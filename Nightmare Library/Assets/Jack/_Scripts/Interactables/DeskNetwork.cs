using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(DeskController))]
public class DeskNetwork : NetworkBehaviour
{
    DeskController parent;

    private void Awake()
    {
        NetworkConnectionController.CheckNetworkConnected(this);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        parent = GetComponent<DeskController>();

        if (!IsOwner)
            parent.enabled = false;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }
}
