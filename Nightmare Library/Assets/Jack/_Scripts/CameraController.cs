using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("GameObjects")]
    public PlayerController playerCont;
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

    // Start is called before the first frame update
    void Start()
    {
        camCont = this;

        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameController.gamePaused)
        {
            MoveCamera();

            // TEMPORARY!!!!
            if (Input.GetKeyDown(KeyCode.R))
            {
                Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
            }
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
        normalCam.enabled = b;
        ghostCam.enabled = !b;
    }
    public void SetEnabled(bool b)
    {
        if (!b)
        {
            normalCam.enabled = false;
            ghostCam.enabled = false;
        }
        else
        {
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

    public bool GetCameraSight(Collider col, float dist)
    {
        float distance = Vector3.SqrMagnitude(col.transform.position - (transform.position - transform.up * -0.15f));

        if (distance <= dist * dist)
        {
            Ray ray = new Ray(normalCam.transform.position, normalCam.transform.forward);
            RaycastHit hit;

            float range = 50;

            if (Physics.Raycast(ray, out hit, range, collideLayers))
            {
                if (hit.collider == col)
                {
                    return true;
                }
            }
        }

        return false;
    }
    public bool GetCameraSight(Collider col)
    {
        Ray ray = new Ray(normalCam.transform.position, normalCam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000, collideLayers))
        {
            if (hit.collider == col)
            {
                return true;
            }
        }

        return false;
    }

    public Ray GetCameraRay()
    {
        return new Ray(transform.position, transform.forward);
    }

    #endregion
}