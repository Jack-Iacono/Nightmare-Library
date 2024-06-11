using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{

    // Modify to use ownership

    public static List<PlayerController> playerInstances = new List<PlayerController>();

    public CameraController camCont { get; private set; }

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

    /*
    [Header("Movement Contstraints", order = 2)]
    [SerializeField]
    private float maxNormalSpeed = 10;
    [SerializeField]
    private float maxCapSpeed = 100;
    */

    [Header("Interaction Variables")]
    public LayerMask environmentLayers;

    private Vector3 currentInput = Vector3.zero;
    private Vector3 currentMove = Vector3.zero;

    private Vector3 previousFramePosition = Vector3.zero;
    private Vector3 velocity = Vector3.zero;

    private CharacterController charCont;

    private void Awake()
    {
        playerInstances.Add(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        charCont = GetComponent<CharacterController>();
        camCont = GetComponentInChildren<CameraController>();

        if (!IsOwner)
        {
            camCont.SetEnabled(false);
            enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        CalculateNormalPlayerMove();
        MovePlayer();
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
    }
    private void CalculateNormalPlayerMove()
    {
        float moveX = currentInput.x * transform.right.x * moveSpeed + currentInput.z * transform.forward.x * moveSpeed;
        float moveZ = currentInput.x * transform.right.z * moveSpeed + currentInput.z * transform.forward.z * moveSpeed;

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
    private void MovePlayer()
    {
        charCont.Move(currentMove * Time.deltaTime);
    }

    public override void OnDestroy()
    {
        //Takes itself out of the player array
        playerInstances.Remove(this);

        base.OnDestroy();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "death_barrier")
        {
            charCont.enabled = false;
            charCont.enabled = true;
        }
    }
}
