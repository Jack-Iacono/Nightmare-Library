using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(UsableController))]
public class UsableNetwork : HoldableItemNetwork
{
    UsableController useableParent;

    protected override void Awake()
    {
        base.Awake();

        useableParent = GetComponent<UsableController>();
        useableParent.OnUse += OnParentUse;
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
        useableParent.Use(true);
        OnParentUseClientRpc(sender);
    }
    [ClientRpc]
    private void OnParentUseClientRpc(ulong sender)
    {
        if(sender != NetworkManager.LocalClientId)
        {
            useableParent.Use();
        }
    }
}
