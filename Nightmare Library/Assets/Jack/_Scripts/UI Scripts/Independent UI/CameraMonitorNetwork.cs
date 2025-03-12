using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CameraMonitorController))]
public class CameraMonitorNetwork : NetworkBehaviour
{
    private CameraMonitorController parent;
    private NetworkVariable<int> cameraIndex = new NetworkVariable<int>();

    private void Awake()
    {
        if (!NetworkConnectionController.connectedToLobby)
        {
            Destroy(this);
            Destroy(GetComponent<NetworkObject>());
        }

        parent = GetComponent<CameraMonitorController>();
        parent.OnStartFinish += OnParentStartFinish;
    }

    private void OnParentStartFinish()
    {
        if (!IsOwner)
            parent.SetCameraIndex(cameraIndex.Value);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            parent.OnCamIndexChange += OnParentIndexChange;
        }
        else
        {
            cameraIndex.OnValueChanged += OnCameraIndexChange;
            parent.ChangeCamera(cameraIndex.Value);
        }
    }

    private void OnParentIndexChange(int index)
    {
        cameraIndex.Value = index;
    }
    private void OnCameraIndexChange(int previousValue, int newValue)
    {
        Debug.Log("New Screen Value " + newValue);
        parent.ChangeCamera(newValue);
    }
}
