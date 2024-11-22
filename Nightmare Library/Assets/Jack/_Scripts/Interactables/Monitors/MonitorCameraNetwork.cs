using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(MonitorCameraController))]
public class MonitorCameraNetwork : InteractableNetwork
{
    private MonitorCameraController monitor;
    private NetworkVariable<bool> isBroadcasting;

    protected override void Awake()
    {
        base.Awake();

        monitor = GetComponent<MonitorCameraController>();

        var permission = NetworkVariableWritePermission.Server;
        isBroadcasting = new NetworkVariable<bool>(writePerm: permission);

        Debug.Log(isBroadcasting.ToString());

        if (NetworkManager.IsServer)
        {
            monitor.OnBroadcastChange += OnBroadcastChange;
        }
        else
        {
            monitor.SetBroadcasting(isBroadcasting.Value);
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        monitor.SetBroadcasting(isBroadcasting.Value);
    }

    private void OnBroadcastChange(object sender, EventArgs e)
    {
        isBroadcasting.Value = monitor.isBroadcasting;
    }
}
