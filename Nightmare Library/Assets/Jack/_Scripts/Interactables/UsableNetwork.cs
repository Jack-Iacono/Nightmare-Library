using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(UsableController))]
public class UsableNetwork : HoldableItemNetwork
{
    UsableController parent;

    protected override void Awake()
    {
        base.Awake();

        parent = GetComponent<UsableController>();
        parent.OnUse += OnParentUse;
    }

    private void OnParentUse(bool fromNetwork)
    {
        if (!fromNetwork)
        {
            if (NetworkManager.IsServer)
            {
                OnParentUseClientRpc(NetworkManager.ServerClientId);
            }
            else
            {
                OnParentUseServerRpc(NetworkManager.LocalClientId);
            }
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void OnParentUseServerRpc(ulong sender)
    {
        parent.Use(true);
        OnParentUseClientRpc(sender);
    }
    [ClientRpc]
    private void OnParentUseClientRpc(ulong sender)
    {
        if(sender != NetworkManager.LocalClientId)
        {
            parent.Use();
        }
    }
}
