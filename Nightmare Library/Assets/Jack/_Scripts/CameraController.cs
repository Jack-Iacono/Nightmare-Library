using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("GameObjects")]
    public PlayerController playerCont;

    // Using 2 different camera for post processing effects later on, could change to layermasks
    public Camera normalCam;
    public Camera ghostCam;

    public AudioListener audioListener;

    //Static instance of this camera
    public static CameraController camCont;

    [Header("Characteristics")]
    [Tooltip("The sensistivity of the mouse moving the camera")]
    public float sensitivity = 100;

    [Header("Layer Masks")]
    public LayerMask collideLayers;
    public LayerMask interactLayers;

    private float xRotation = 0;
    private float yRotation = 0;

    private Vector3 normalPosition = Vector3.zero;

    private bool isLocked = false;
    private bool camMoved = false;

    private void Awake()
    {
        // Sets the normal position that the camera should be in
        normalPosition = transform.localPosition;
    }

    // Start is called before the first frame update
    void Start()
    {
        camCont = this;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        // Check to make sure the game isn't paused
        if (!GameController.gamePaused)
        {
            // Move the camera
            if(!isLocked)
                MoveCamera();
        }
    }

    public void Lock(bool locked)
    {
        if(!locked)
        {
            //transform.localPosition = normalPosition;
            //transform.localRotation = Quaternion.identity;
            StopAllCoroutines();    
            StartCoroutine(ResetCamera());
            camMoved = false;
        }

        isLocked = locked;
    }
    public void Lock(bool locked, Transform camTransform)
    {
        //transform.position = camTransform.position;
        //transform.rotation = camTransform.rotation;
        StopAllCoroutines();
        StartCoroutine(MoveCamera(camTransform));

        camMoved = true;

        Lock(locked);
    }

    private IEnumerator MoveCamera(Transform movePoint)
    {
        while (Vector3.SqrMagnitude(movePoint.position - transform.position) > 0.00005f)
        {
            transform.position = Vector3.Slerp(transform.position, movePoint.position, 8 * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, movePoint.rotation, 8 * Time.deltaTime);

            yield return null;
        }
    }
    private IEnumerator ResetCamera()
    {
        while(Vector3.SqrMagnitude(normalPosition - transform.localPosition) > 0.00005f)
        {
            transform.localPosition = Vector3.Slerp(transform.localPosition, normalPosition, 8 * Time.deltaTime);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, 8 * Time.deltaTime);

            yield return null;
        }
    }

    #region Camera Function Methods

    private void MoveCamera()
    {
        //Taking in the input from the mouse
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        //Gets the real rotation of the camera
        xRotation = xRotation - mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //Getting the horizontal rotations from mouse inputs
        yRotation = yRotation + mouseX;

        //Moves the camera around the player
        transform.localRotation = Quaternion.Euler(Mathf.Clamp(xRotation, -90, 90), 0f, 0f);

        //Rotates the player to always be facing the direction of the camera
        playerCont.transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
    }
    public void SetGhost(bool b)
    {
        // Switches the active camera, ghosts see different layers than players
        normalCam.enabled = !b;
        ghostCam.enabled = b;
    }
    public void SetEnabled(bool b)
    {
        // Sets whether the camera is in use, not being used much anymore since spectate rework
        if (!b)
        {
            normalCam.enabled = false;
            ghostCam.enabled = false;
        }
        else
        {
            // Checks if the plaeyr is alive to determine which camera to use
            if (playerCont.isAlive)
            {
                normalCam.enabled = true;
                ghostCam.enabled = false;
            }
            else
            {
                normalCam.enabled = false;
                ghostCam.enabled = true;
            }
        }

        audioListener.enabled = b;
        enabled = b;
    }

    #endregion

    #region Get Methods

    public bool GetCameraSight(Collider col)
    {
        // Get a ray from the camera's position in a straight line forward
        Ray ray = new Ray(normalCam.transform.position, normalCam.transform.forward);
        RaycastHit hit;

        // Check if the ray hits any layers that we are interested in
        if (Physics.Raycast(ray, out hit, 1000, collideLayers))
        {
            // If the collider is the one we are looking for, return true
            if (hit.collider == col)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets a ray straight forward from the camera's position
    /// </summary>
    /// <returns>The ray representing the camera's sightline</returns>
    public Ray GetCameraRay()
    {
        return new Ray(transform.position, transform.forward);
    }

    #endregion
}