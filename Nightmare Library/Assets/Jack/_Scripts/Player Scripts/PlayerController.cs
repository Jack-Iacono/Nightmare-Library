using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController ownerInstance;

    public static List<PlayerController> playerInstances = new List<PlayerController>();

    private static int currentlySpectating;
    private int myPlayerIndex;

    public static LayerMask playerLayerMask;

    public CameraController camCont;

    private Collider playerCollider;
    [SerializeField]
    private List<MeshRenderer> playerMeshes;

    [SerializeField]
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
    public static event EventHandler OnPlayerKilled;

    private bool isTrapped = false;
    private float trapTimer = 0;

    private void Awake()
    {
        playerInstances.Add(this);
        myPlayerIndex = playerInstances.Count - 1;

        charCont = GetComponent<CharacterController>();
        playerCollider = GetComponent<Collider>();

        // TEMPORARY
        charCont.enabled = false;
        transform.position = new Vector3(-20, 1, 0);
        charCont.enabled = true;

        if (!NetworkConnectionController.IsOnline)
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

            float accelX = moveX == 0 ? groundDeceleration : groundAcceleration;
            float accelZ = moveZ == 0 ? groundDeceleration : groundAcceleration;

            currentMove.x = Mathf.MoveTowards(currentMove.x, moveX, accelX * Time.deltaTime);
            currentMove.z = Mathf.MoveTowards(currentMove.z, moveZ, accelZ * Time.deltaTime);
        }
        else
        {
            // Sets into fall if hitting a ceiling
            if (Physics.Raycast(transform.position, Vector3.up, 1.1f, environmentLayers) && currentMove.y > 0)
                currentMove.y = 0;

            currentMove.y -= gravity * -2 * Time.deltaTime;

            float accelX = moveX == 0 ? airDeceleration : airAcceleration;
            float accelZ = moveZ == 0 ? airDeceleration : airAcceleration;

            currentMove.x = Mathf.MoveTowards(currentMove.x, moveX, accelX * Time.deltaTime);
            currentMove.z = Mathf.MoveTowards(currentMove.z, moveZ, accelZ * Time.deltaTime);
        }
    }
    private void Move()
    {
        charCont.Move(currentMove * Time.deltaTime);
    }

    public void OnDestroy()
    {
        //Takes itself out of the player array
        playerInstances.Remove(this);
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
        if (!NetworkConnectionController.IsOnline)
        {
            Kill();
        }
        OnPlayerAttacked?.Invoke(this, EventArgs.Empty);
    }
    public void Kill()
    {
        Activate(false);
        isAlive = false;

        playerCollider.enabled = false;
        charCont.enabled = false;
        foreach(MeshRenderer r in playerMeshes)
        {
            r.enabled = false;
        }

        OnPlayerKilled?.Invoke(this, EventArgs.Empty);

        int aliveIndex = GetAlivePlayer();

        if (aliveIndex != -1)
            SpectatePlayer(aliveIndex);
        else
            camCont.SetEnabled(true);
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
        //charCont.enabled = b;

        if (b)
        {
            name = "My Player";
            ownerInstance = this;
        }
    }

    public static void SpectatePlayer(int index)
    {
        if (playerInstances[index].isAlive)
        {
            playerInstances[currentlySpectating].camCont.Spectate(false);
            playerInstances[index].camCont.Spectate(true);
            currentlySpectating = index;
        }
    }
    private int GetAlivePlayer()
    {
        for(int i = 0; i < playerInstances.Count; i++)
        {
            if (playerInstances[i].isAlive)
                return i;
        }

        return -1;
    }
}
