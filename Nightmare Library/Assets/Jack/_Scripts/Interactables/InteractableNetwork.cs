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

        if(parent.allowPlayerClick)
            parent.OnClick += OnClick;
        if (parent.allowPlayerPickup)
        {
            parent.OnPickup += OnPickup;

            // These options have to be available if the player can pick the item up
            parent.OnPlace += OnPlace;
            parent.OnThrow += OnThrow;
        }
        
        if(parent.allowEnemyHysterics)
            parent.OnEnemyInteractHysterics += OnEnemyInteractHysterics;
        if(parent.allowEnemyFlicker)
            parent.OnEnemyInteractFlicker += OnEnemyInteractFlicker;
    }

    #region Click
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
    #endregion

    #region Pickup

    protected virtual void OnPickup(bool fromNetwork)
    {
        if (!fromNetwork)
        {
            if (IsOwner)
                ConsumePickupClientRpc(NetworkManager.LocalClientId);
            else
                TransmitPickupServerRpc(NetworkManager.LocalClientId);
        }
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
            parent.Pickup(true);
    }

    #endregion

    #region Place
    protected virtual void OnPlace(bool fromNetwork)
    {
        if (!fromNetwork)
        {
            if (IsOwner)
                ConsumePlaceClientRpc(NetworkManager.LocalClientId, transform.position, transform.rotation);
            else
                TransmitPlaceServerRpc(NetworkManager.LocalClientId, transform.position, transform.rotation);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    protected virtual void TransmitPlaceServerRpc(ulong sender, Vector3 pos, Quaternion rot)
    {
        ConsumePlaceClientRpc(sender, pos, rot);
    }
    [ClientRpc]
    protected virtual void ConsumePlaceClientRpc(ulong sender, Vector3 pos, Quaternion rot)
    {
        if (NetworkManager.LocalClientId != sender)
            parent.Place(pos, rot, false);
    }
    #endregion

    #region Throw
    protected virtual void OnThrow(bool fromNetwork)
    {
        throw new NotImplementedException();
    }
    [ServerRpc(RequireOwnership = false)]
    protected virtual void TransmitThrowServerRpc(ulong sender)
    {
        ConsumeThrowClientRpc(sender);
    }
    [ClientRpc]
    protected virtual void ConsumeThrowClientRpc(ulong sender)
    {
        if (NetworkManager.LocalClientId != sender)
            Debug.Log("Throw on client " + sender);
    }
    #endregion

    #region Enemy Interact Hysterics
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
    #endregion

    #region Enemy Interact Flicker
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
    #endregion
}
