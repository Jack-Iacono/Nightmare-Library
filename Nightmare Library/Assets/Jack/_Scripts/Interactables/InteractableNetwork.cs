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
        parent.OnPickup += OnPickup;
        parent.OnPlace += OnPlace;
        parent.OnEnemyInteractHysterics += OnEnemyInteractHysterics;
        parent.OnEnemyInteractFlicker += OnEnemyInteractFlicker;
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

    protected virtual void OnPickup(bool fromNetwork)
    {
        throw new NotImplementedException();
    }
    [ServerRpc(RequireOwnership = false)]
    protected virtual void TransmitPickupServerRpc(ulong sender)
    {
        ConsumePickupClientRpc(sender);
    }
    [ClientRpc]
    protected virtual void ConsumePickupClientRpc(ulong sender)
    {
        if (NetworkManager.LocalClientId != sender)
            Debug.Log("Click on client " + sender);
    }

    protected virtual void OnPlace(bool fromNetwork)
    {
        throw new NotImplementedException();
    }
    [ServerRpc(RequireOwnership = false)]
    protected virtual void TransmitPlaceServerRpc(ulong sender)
    {
        ConsumePlaceClientRpc(sender);
    }
    [ClientRpc]
    protected virtual void ConsumePlaceClientRpc(ulong sender)
    {
        if (NetworkManager.LocalClientId != sender)
            Debug.Log("Click on client " + sender);
    }

    protected virtual void OnEnemyInteractHysterics(bool fromNetwork)
    {
        throw new NotImplementedException();
    }
    [ServerRpc(RequireOwnership = false)]
    protected virtual void TransmitEnemyInteractHystericsServerRpc(ulong sender)
    {
        ConsumeEnemyInteractHystericsClientRpc(sender);
    }
    [ClientRpc]
    protected virtual void ConsumeEnemyInteractHystericsClientRpc(ulong sender)
    {
        if (NetworkManager.LocalClientId != sender)
            Debug.Log("Click on client " + sender);
    }

    protected virtual void OnEnemyInteractFlicker(bool fromNetwork)
    {
        throw new NotImplementedException();
    }
    [ServerRpc(RequireOwnership = false)]
    protected virtual void TransmitEnemyInteractFlickerServerRpc(ulong sender)
    {
        ConsumeEnemyInteractFlickerClientRpc(sender);
    }
    [ClientRpc]
    protected virtual void ConsumeEnemyInteractFlickerClientRpc(ulong sender)
    {
        if (NetworkManager.LocalClientId != sender)
            Debug.Log("Click on client " + sender);
    }
}
