using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController mainPlayerInstance;
    public static Dictionary<GameObject, PlayerController> playerInstances = new Dictionary<GameObject, PlayerController>();

    public static LayerMask playerLayerMask;

    private const int playerLayer = 6;
    private const int ghostLayer = 14;

    public CameraController camCont;
    public AudioSourceController audioSource;
    private PlayerInteractionController interactionCont;

    [Header("Mesh / Material")]
    [SerializeField]
    private List<MeshMaterialLink> meshMaterials;

    [NonSerialized]
    public bool isAlive = true;

    [Header("Movement Variables")]
    [SerializeField]
    private float moveSpeed = 10;
    [SerializeField]
    private float jumpHeight = 10;
    [SerializeField]
    [Tooltip("Negative values will pull player downward, Positive value will push them up")]
    private float gravity = -0.98f;

    private bool isLocked = false;

    [Header("Acceleration Variables", order = 2)]
    [SerializeField]
    private float groundAcceleration = 1;
    [SerializeField]
    private float airAcceleration = 1;
    [SerializeField]
    private float groundDeceleration = 1;
    [SerializeField]
    private float airDeceleration = 1;

    [Header("Interaction Variables")]
    public LayerMask environmentLayers;

    private Vector3 currentInput = Vector3.zero;
    private Vector3 currentMove = Vector3.zero;

    private Vector3 previousFramePosition = Vector3.zero;
    private Vector3 velocity = Vector3.zero;

    private CharacterController charCont;

    private KeyCode keySprint = KeyCode.LeftShift;
    private bool isSprinting = false;

    public delegate void OnPlayerAliveChangedDelegate(PlayerController player, bool b);
    public static event OnPlayerAliveChangedDelegate OnPlayerAliveChanged;

    private bool isTrapped = false;
    private float trapTimer = 0;

    private void Awake()
    {
        // Add the player to the list of player instances present in the game
        playerInstances.Add(gameObject, this);

        // Get components on the player
        charCont = GetComponent<CharacterController>();
        interactionCont = GetComponent<PlayerInteractionController>();

        // Make all meshes on the player normal
        for (int i = 0; i < meshMaterials.Count; i++)
        {
            meshMaterials[i].renderer.material = meshMaterials[i].normal;
        }

        // Set this instance to the main instance if the game is not networked
        if (!NetworkConnectionController.connectedToLobby)
            mainPlayerInstance = this;

        // Teleport the player to the spawn point of the map
        Warp(MapDataController.Instance.playerSpawnPoint);

        // Get the player's layer from the editor
        playerLayerMask = gameObject.layer;
    }
    public void Warp(Vector3 pos)
    {
        // Disabling the character controller allows the player to be warped, otherwise it doesn't work
        charCont.enabled = false;
        transform.position = pos;
        charCont.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!PauseController.gamePaused)
        {
            if (!isTrapped)
            {
                GetInput();
                if (!isLocked)
                {
                    CalculateNormalMove();
                    Move();
                }
            }
        }

        if (isTrapped)
        {
            if (trapTimer > 0)
                trapTimer -= Time.deltaTime;
            else
                isTrapped = false;
        }
    }
    private void FixedUpdate()
    {
        velocity = (transform.position - previousFramePosition) / Time.fixedDeltaTime;
        previousFramePosition = transform.position;
    }

    private void GetInput()
    {
        currentInput = new Vector3
            (
                Input.GetAxis("Horizontal"),
                Input.GetButtonDown("Jump") ? 1 : 0,
                Input.GetAxis("Vertical")
            );
        isSprinting = Input.GetKey(keySprint);

        if (Input.GetKeyDown(KeyCode.K))
        {
            interactionCont.DropItems();
        }
    }
    private void CalculateNormalMove()
    {
        float moveX = currentInput.x * transform.right.x * moveSpeed + currentInput.z * transform.forward.x * moveSpeed;
        float moveZ = currentInput.x * transform.right.z * moveSpeed + currentInput.z * transform.forward.z * moveSpeed;

        // TEMPORARY
        if (isSprinting)
        {
            moveX *= 2f;
            moveZ *= 2f;
        }

        if (charCont.isGrounded)
        {
            if (currentInput.y != 0)
            {
                if(isAlive)
                    audioSource.Play(AudioManager.GetAudioData(AudioManager.SoundType.p_JUMP));
                currentMove.y = jumpHeight;
            }

            // Decide whether to use the accel or decel for the player given the presence of input
            float accelX = moveX == 0 ? groundDeceleration : groundAcceleration;
            float accelZ = moveZ == 0 ? groundDeceleration : groundAcceleration;

            // Old movement didn't move numbers at equal rates
            //currentMove.x = Mathf.MoveTowards(currentMove.x, moveX, accelX * Time.deltaTime);
            //currentMove.z = Mathf.MoveTowards(currentMove.z, moveZ, accelZ * Time.deltaTime);

            // Change the player's current movement vector to reflect the changes made through input
            currentMove.x = Mathf.Lerp(currentMove.x, moveX, accelX);
            currentMove.z = Mathf.Lerp(currentMove.z, moveZ, accelZ);
        }
        else
        {
            // Sets into fall if hitting a ceiling
            if (Physics.Raycast(transform.position, Vector3.up, 1.1f, environmentLayers) && currentMove.y > 0)
                currentMove.y = 0;

            currentMove.y -= gravity * -2 * Time.deltaTime;

            // Decide whether to use the accel or decel for the player given the presence of input
            float accelX = moveX == 0 ? airDeceleration : airAcceleration;
            float accelZ = moveZ == 0 ? airDeceleration : airAcceleration;

            // Old movement didn't move numbers at equal rates
            //currentMove.x = Mathf.MoveTowards(currentMove.x, moveX, accelX * Time.deltaTime);
            //currentMove.z = Mathf.MoveTowards(currentMove.z, moveZ, accelZ * Time.deltaTime);

            // Change the player's current movement vector to reflect the changes made through input
            currentMove.x = Mathf.Lerp(currentMove.x, moveX, accelX);
            currentMove.z = Mathf.Lerp(currentMove.z, moveZ, accelZ);
        }
    }
    private void Move()
    {
        charCont.Move(currentMove * Time.deltaTime);
    }

    public void OnDestroy()
    {
        //Takes itself out of the player array
        OnPlayerAliveChanged?.Invoke(this, false);
        playerInstances.Remove(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "death_barrier")
        {
            charCont.enabled = false;
            charCont.enabled = true;
        }
    }

    public void ChangeAliveState(bool alive)
    {
        isAlive = alive;

        if(!alive)
        {
            // Kill
            for (int i = 0; i < meshMaterials.Count; i++)
            {
                meshMaterials[i].renderer.material = meshMaterials[i].ghost;
                meshMaterials[i].renderer.gameObject.layer = ghostLayer;
            }

            gameObject.layer = ghostLayer;

            // Exectue only if this is the main player instance for this machine
            if (mainPlayerInstance == this)
            {
                camCont.SetGhost(true);
                interactionCont.enabled = false;
                interactionCont.DropItems();
            }
        }
        else
        {
            // Resurrect
            for (int i = 0; i < meshMaterials.Count; i++)
            {
                meshMaterials[i].renderer.material = meshMaterials[i].normal;
                meshMaterials[i].renderer.gameObject.layer = playerLayer;
            }

            gameObject.layer = playerLayer;

            if (mainPlayerInstance == this)
            {
                camCont.SetGhost(false);
                interactionCont.enabled = true;
            }
        }

        OnPlayerAliveChanged?.Invoke(this, alive);
    }

    public void Trap(float duration)
    {
        isTrapped = true;
        trapTimer = duration;
    }

    public void Activate(bool b)
    {
        enabled = b;
        camCont.SetEnabled(b);
        interactionCont.enabled = b;

        if (b)
        {
            name = "My Player";
            mainPlayerInstance = this;
        }
    }

    public void Lock(bool b)
    {
        isLocked = b;

        if(camCont != null)
            camCont.Lock(b);
    }
    public void Lock(bool b, Transform camTransform)
    {
        isLocked = b;
        camCont.Lock(b, camTransform);
    }

    public bool CheckMoveInput()
    {
        // Checks whether there is move input for this frame
        return currentInput != Vector3.zero;
    }

    [Serializable]
    public class MeshMaterialLink
    {
        public MeshRenderer renderer;
        public Material normal;
        public Material ghost;
    }
    
}
