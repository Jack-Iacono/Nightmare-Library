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
        if (NetworkConnectionController.CheckNetworkConnected(this))
        {
            parent = GetComponent<CameraMonitorController>();
            parent.OnStartFinish += OnParentStartFinish;
        }
    }

    private void OnParentStartFinish()
    {
        if (!IsOwner)
            parent.SetCameraIndex(cameraIndex.Value);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner)
        {
            cameraIndex.OnValueChanged += OnCameraIndexChange;
            parent.ChangeCamera(cameraIndex.Value);
        }

        parent.OnCamIndexChange += OnParentIndexChange;
    }

    private void OnParentIndexChange(int index, bool fromNetwork = false)
    {
        if (!fromNetwork)
        {
            if (IsOwner)
                cameraIndex.Value = index;
            else
                OnParentIndexChangeServerRpc(index);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void OnParentIndexChangeServerRpc(int index)
    {
        parent.SetCameraIndex(index);
        cameraIndex.Value = index;
    }

    private void OnCameraIndexChange(int previousValue, int newValue)
    {
        parent.ChangeCamera(newValue);
    }
}
