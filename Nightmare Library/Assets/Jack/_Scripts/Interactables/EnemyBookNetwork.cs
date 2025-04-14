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
        parent = GetComponent<EnemyBookController>();
        parent.OnAppliedBookChanged += OnAppliedBookChanged;
    }

    private void OnAppliedBookChanged(PlayerController controller, EnemyPreset preset)
    {
        if (NetworkManager.IsServer)
        {
            EnemyBookController.ChangeAppliedBook(controller, preset);
        }
        else
        {
            // Send the data over that can be decoded later
            PlayerNetwork network = PlayerNetwork.playerNetworkReference[controller];
            OnAppliedBookChangedServerRpc(network.NetworkObjectId, PersistentDataController.Instance.enemyPresets.IndexOf(preset));
        }
    }
    [ServerRpc]
    private void OnAppliedBookChangedServerRpc(ulong networkObjectId, int enemyPreset)
    {
        PlayerNetwork network = GetNetworkObject(networkObjectId).GetComponent<PlayerNetwork>();
        OnAppliedBookChanged(PlayerNetwork.playerNetworkReference[network], PersistentDataController.Instance.enemyPresets[enemyPreset]);
    }
}
