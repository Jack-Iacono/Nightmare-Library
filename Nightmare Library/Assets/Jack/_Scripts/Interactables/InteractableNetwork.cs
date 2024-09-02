using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class InteractableNetwork : NetworkBehaviour
{
    protected Interactable parent;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        parent = GetComponent<Interactable>();

        parent.OnClick += OnClick;
    }

    protected virtual void OnClick(bool fromNetwork = false)
    {
        if (!fromNetwork)
        {
            if (IsOwner)
                ConsumeClickClientRpc(NetworkManager.LocalClientId);
            else
                TransmitClickServerRpc(NetworkManager.LocalClientId);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    protected virtual void TransmitClickServerRpc(ulong sender)
    {
        ConsumeClickClientRpc(sender);
    }
    [ClientRpc]
    protected virtual void ConsumeClickClientRpc(ulong sender)
    {
        if (NetworkManager.LocalClientId != sender)
            Debug.Log("Click on client " + sender);
    }
}
