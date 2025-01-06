using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(MonitorController))]
public class MonitorNetwork : NetworkBehaviour
{
    private MonitorController monitor;
    private NetworkVariable<int> cameraIndex;

    private void Awake()
    {
        monitor = GetComponent<MonitorController>();

        var permission = NetworkVariableWritePermission.Server;
        cameraIndex = new NetworkVariable<int>(writePerm: permission);

        if (IsServer)
        {
            monitor.onCamIndexChange += OnParentIndexChange;
        }
        else
        {
            cameraIndex.OnValueChanged += OnCameraIndexChange;
        }
    }

    private void OnParentIndexChange(int index)
    {
        Debug.Log("Changing Camera");
        cameraIndex.Value = index;
    }
    private void OnCameraIndexChange(int previousValue, int newValue)
    {
        Debug.Log("New Camera Value " + newValue);
        monitor.SetCameraIndex(newValue);
    }
}
