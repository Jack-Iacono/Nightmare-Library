using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(moui_CameraScreenController))]
public class moui_CameraScreenNetwork : NetworkBehaviour
{
    private moui_CameraScreenController parent;
    private NetworkVariable<int> cameraIndex = new NetworkVariable<int>();

    private void Awake()
    {
        if (!NetworkConnectionController.connectedToLobby)
        {
            Destroy(this);
            Destroy(GetComponent<NetworkObject>());
        }

        parent = GetComponent<moui_CameraScreenController>();
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
