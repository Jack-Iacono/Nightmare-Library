using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(EnemyBookController))]
public class EnemyBookNetwork : NetworkBehaviour
{
    private EnemyBookController parent;
    private void Awake()
    {
        if (NetworkConnectionController.CheckNetworkConnected(this))
        {
            parent = GetComponent<EnemyBookController>();
            parent.OnAppliedBookChanged += OnAppliedBookChanged;
        }
    }


    private void OnAppliedBookChanged(PlayerController controller, EnemyPreset preset)
    {
        if (NetworkManager.IsServer)
        {
            EnemyBookController.ChangeAppliedBook(controller, preset);
        }
        else
        {
            Debug.Log("Sending Server Rpc");

            // Send the data over that can be decoded later
            OnAppliedBookChangedServerRpc(PlayerNetwork.playerNetworkReference[controller].NetworkObjectId, PersistentDataController.Instance.enemyPresets.IndexOf(preset));
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void OnAppliedBookChangedServerRpc(ulong networkObjectId, int enemyPreset)
    {
        OnAppliedBookChanged(PlayerNetwork.playerNetworkReference[GetNetworkObject(networkObjectId)], PersistentDataController.Instance.enemyPresets[enemyPreset]);
    }
}
