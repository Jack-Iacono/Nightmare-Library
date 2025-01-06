using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonitorController : MonoBehaviour
{
    [Header("Monitor Variabels")]
    public GameObject monitorViewBlocker;
    public RawImage monitorPicture;

    public List<MonitorCameraController> linkedCameras = new List<MonitorCameraController>();
    private int cameraIndex = 0;

    public float useRadius = 5f;
    private float playerCheckTime = 0.25f;
    private float playerCheckTimer = 0.25f;
    private LayerMask playerMask = 1 << 6;

    public delegate void OnCamIndexChangeDelegate(int index);
    public OnCamIndexChangeDelegate onCamIndexChange;

    private RenderTexture display;

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < linkedCameras.Count; i++)
        {
            linkedCameras[i].SetViewing(false);
            linkedCameras[i].OnBroadcastChange += OnCameraBroadcastChange;
        }

        ChangeCamera(0);
        playerCheckTimer = playerCheckTime;
    }

    // Update is called once per frame
    void Update()
    {
        if(playerCheckTimer > 0)
            playerCheckTimer -= Time.deltaTime;
        else
        {
            CheckPlayerInRange();
            playerCheckTimer = playerCheckTime;
        }
    }

    private void OnCameraBroadcastChange(bool broadcasting)
    {
        CheckPlayerInRange();
    }

    private void CheckPlayerInRange()
    {
        if (linkedCameras[cameraIndex].isBroadcasting)
        {
            bool inRange = Physics.OverlapSphere(transform.position, useRadius, playerMask).Length > 0;
            monitorViewBlocker.SetActive(!inRange || !linkedCameras[cameraIndex].isBroadcasting);
        }
        else
        {
            // Checks to see if any other cameras are broadcasting
            bool found = false;
            for(int i = 0; i < linkedCameras.Count; i++)
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
                monitorViewBlocker.SetActive(true);
            }
            
        }
    }
    public void ChangeCamera(int i)
    {
        linkedCameras[cameraIndex].SetViewing(false);
        cameraIndex = i;
        linkedCameras[cameraIndex].SetViewing(true);

        display = linkedCameras[cameraIndex].renderTexture;
        monitorPicture.texture = display;

        CheckPlayerInRange();
    }

    public void SetCameraIndex(int i)
    {
        cameraIndex = i;
        ChangeCamera(cameraIndex);
    }

    public void Click(bool upDown)
    {
        cameraIndex = (cameraIndex + (upDown ? 1 : -1)) % linkedCameras.Count;
        cameraIndex = cameraIndex < 0 ? linkedCameras.Count - 1 : cameraIndex;
        ChangeCamera(cameraIndex);

        Debug.Log("New Camera Index " + cameraIndex);

        onCamIndexChange?.Invoke(cameraIndex);
    }
}
