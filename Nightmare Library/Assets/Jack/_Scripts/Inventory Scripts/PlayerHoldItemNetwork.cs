using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerHoldItemController))]
public class PlayerHoldItemNetwork : NetworkBehaviour
{
    private PlayerHoldItemController parent;
    private ulong heldObjectId;

    private void Awake()
    {
        if (NetworkConnectionController.CheckNetworkConnected(this))
        {
            parent = GetComponent<PlayerHoldItemController>();

            parent.OnHeldItemChanged += OnHeldItemChanged;
        }
    }

    private void OnHeldItemChanged(HoldableItem item)
    {
        if (NetworkManager.IsServer)
            OnHeldItemChangedClientRpc(HoldableItemNetwork.idLink[item]);
        else
            OnHeldItemChangedServerRpc(HoldableItemNetwork.idLink[item]);
    }
    [ServerRpc]
    private void OnHeldItemChangedServerRpc(ulong id)
    {
        
    }
    [ClientRpc]
    private void OnHeldItemChangedClientRpc(ulong id)
    {
        if (!NetworkManager.IsServer)
        {

        }
    }
}
