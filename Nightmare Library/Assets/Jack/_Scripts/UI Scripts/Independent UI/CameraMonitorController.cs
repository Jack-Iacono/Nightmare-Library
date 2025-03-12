using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CameraMonitorController : MonoBehaviour
{
    [Header("Monitor Variabels")]
    [SerializeField]
    private TMP_Text cameraText;
    public GameObject cameraNoSignalScreen;
    public RawImage cameraPicture;

    public List<LibraryCameraController> linkedCameras = new List<LibraryCameraController>();
    private int cameraIndex = 0;

    public float useRadius = 5f;
    private float playerCheckTime = 0.25f;
    private float playerCheckTimer = 0.25f;
    private LayerMask playerMask = 1 << 6;

    public delegate void OnCamIndexChangeDelegate(int index);
    public OnCamIndexChangeDelegate OnCamIndexChange;

    public delegate void OnStartFinishDelegate();
    public OnStartFinishDelegate OnStartFinish;

    private RenderTexture display;

    // Start is called before the first frame update
    protected void Start()
    {
        for (int i = 0; i < linkedCameras.Count; i++)
        {
            linkedCameras[i].SetViewing(false);
            linkedCameras[i].OnBroadcastChange += OnCameraBroadcastChange;
        }

        ChangeCamera(0);
        playerCheckTimer = playerCheckTime;

        OnStartFinish?.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerCheckTimer > 0)
            playerCheckTimer -= Time.deltaTime;
        else
        {
            CheckCameraBroadcasting();
            playerCheckTimer = playerCheckTime;
        }
    }

    private void OnCameraBroadcastChange(bool broadcasting)
    {
        CheckCameraBroadcasting();
    }

    private void CheckCameraBroadcasting()
    {
        if (linkedCameras[cameraIndex].isBroadcasting)
        {
            cameraNoSignalScreen.SetActive(false);
        }
        else
        {
            // Checks to see if any other cameras are broadcasting
            bool found = false;
            for (int i = 0; i < linkedCameras.Count; i++)
            {
                if (linkedCameras[i].isBroadcasting)
                {
                    ChangeCamera(i);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Debug.Log("No camera broadcasting");
                cameraNoSignalScreen.SetActive(true);
            }
        }
    }
    public void ChangeCamera(int i)
    {
        linkedCameras[cameraIndex].SetViewing(false);
        cameraIndex = i;
        linkedCameras[cameraIndex].SetViewing(true);

        display = linkedCameras[cameraIndex].renderTexture;
        cameraPicture.texture = display;

        cameraText.text = "Cam " + cameraIndex;

        CheckCameraBroadcasting();
        OnCamIndexChange?.Invoke(cameraIndex);
    }

    public void SetCameraIndex(int i)
    {
        cameraIndex = i;
        ChangeCamera(cameraIndex);
    }

    public void NextCamera()
    {
        cameraIndex = (cameraIndex + 1) % linkedCameras.Count;
        ChangeCamera(cameraIndex);
    }
}
