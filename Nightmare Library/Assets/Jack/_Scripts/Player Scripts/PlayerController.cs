using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class PlayerController : MonoBehaviour
{
    public static PlayerController ownerInstance;

    public static Dictionary<GameObject, PlayerController> playerInstances = new Dictionary<GameObject, PlayerController>();

    public static LayerMask playerLayerMask;

    private const int playerLayer = 6;
    private const int ghostLayer = 14;

    public CameraController camCont;
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

    public event EventHandler OnPlayerAttacked;
    public delegate void PlayerKilledDelegate(PlayerController player);
    public static event PlayerKilledDelegate OnPlayerKilled;

    private bool isTrapped = false;
    private float trapTimer = 0;

    private void Awake()
    {
        playerInstances.Add(gameObject, this);

        charCont = GetComponent<CharacterController>();
        interactionCont = GetComponent<PlayerInteractionController>();

        // TEMPORARY
        charCont.enabled = false;
        transform.position = new Vector3(-20, 1, 0);
        charCont.enabled = true;

        for (int i = 0; i < meshMaterials.Count; i++)
        {
            meshMaterials[i].renderer.material = meshMaterials[i].normal;
        }

        if (!NetworkConnectionController.connectedToLobby)
            ownerInstance = this;

        playerLayerMask = gameObject.layer;
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameController.gamePaused)
        {
            if (!isTrapped)
            {
                GetInput();
                CalculateNormalMove();
                Move();
            }
            else
            {
                if (trapTimer > 0)
                    trapTimer -= Time.deltaTime;
                else
                    isTrapped = false;
            }
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
        OnPlayerKilled?.Invoke(this);
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

    public void ReceiveAttack()
    {
        if (!NetworkConnectionController.connectedToLobby)
        {
            Kill(true);
        }
        OnPlayerAttacked?.Invoke(this, EventArgs.Empty);
    }
    public void Kill(bool becomeGhost)
    {
        isAlive = false;

        for (int i = 0; i < meshMaterials.Count; i++)
        {
            meshMaterials[i].renderer.material = meshMaterials[i].ghost;
            meshMaterials[i].renderer.gameObject.layer = ghostLayer;
        }

        gameObject.layer = ghostLayer;
        charCont.excludeLayers = playerLayer | 1 << 15;

        OnPlayerKilled?.Invoke(this);

        interactionCont.enabled = false;
        interactionCont.DropItems();

        if(becomeGhost)
            camCont.SetGhost(false);
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
            ownerInstance = this;
        }
    }
    public void Lock(bool b)
    {
        enabled = !b;
        camCont.enabled = !b;
    }

    [Serializable]
    public class MeshMaterialLink
    {
        public MeshRenderer renderer;
        public Material normal;
        public Material ghost;
    }
    
}
