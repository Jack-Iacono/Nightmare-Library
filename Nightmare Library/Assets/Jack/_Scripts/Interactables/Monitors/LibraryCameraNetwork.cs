using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(LibraryCameraController))]
public class LibraryCameraNetwork : InteractableNetwork
{
    private LibraryCameraController monitor;
    private NetworkVariable<bool> isBroadcasting;

    protected override void Awake()
    {
        base.Awake();

        monitor = GetComponent<LibraryCameraController>();

        var permission = NetworkVariableWritePermission.Server;
        isBroadcasting = new NetworkVariable<bool>(writePerm: permission);

        if (NetworkManager.IsServer)
        {
            monitor.OnBroadcastChange += OnBroadcastChange;
        }
        else
        {
            isBroadcasting.OnValueChanged += OnBroadcastNetworkUpdate;
        }
    }

    

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (NetworkManager.IsServer)
        {
            isBroadcasting.Value = true;
        }
        monitor.SetBroadcasting(isBroadcasting.Value);
    }

    private void OnBroadcastNetworkUpdate(bool previousValue, bool newValue)
    {
        monitor.SetBroadcasting(newValue);
    }
    private void OnBroadcastChange(bool broadcasting)
    {
        isBroadcasting.Value = broadcasting;
    }
}
