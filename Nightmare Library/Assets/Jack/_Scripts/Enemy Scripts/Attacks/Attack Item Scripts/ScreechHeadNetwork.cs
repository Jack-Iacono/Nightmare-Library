using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(ScreechHeadController))]
public class ScreechHeadNetwork : NetworkBehaviour
{
    ScreechHeadController parent;

    private void Awake()
    {
        if (!NetworkConnectionController.connectedToLobby)
        {
            Destroy(this);
            Destroy(GetComponent<NetworkObject>());
        }

        parent = GetComponent<ScreechHeadController>();

        if (NetworkManager.IsServer)
        {
            parent.OnSpawnHead += OnSpawnHead;
            parent.OnAttack += OnAttack;
        }

        parent.OnDespawnHead += OnDespawnHead;
        parent.OnInitialize += OnInitialize;
    }

    public void OnInitialize()
    {
        if (IsServer)
        {
            // Sets this object to have the correct owner
            GetComponent<NetworkObject>().ChangeOwnership(parent.targetPlayer.GetComponent<NetworkObject>().OwnerClientId);
            OnInitializeClientRpc();
        }
    }
    [ClientRpc]
    private void OnInitializeClientRpc()
    {
        foreach (PlayerController p in PlayerController.playerInstances.Values)
        {
            if (p.GetComponent<PlayerNetwork>().OwnerClientId == OwnerClientId)
            {
                parent.Initialize(p, true, NetworkManager.IsServer);
                break;
            }
        }
    }

    public void OnAttack()
    {
        if (IsServer)
            OnAttackClientRpc(NetworkManager.LocalClientId);
    }
    [ClientRpc]
    public void OnAttackClientRpc(ulong sender)
    {
        if(sender != NetworkManager.LocalClientId)
            parent.Attack();
    }

    public void OnSpawnHead(Vector3 offset)
    {
        Debug.Log("Spawning Head on Server " + IsServer);
        if (IsServer)
            OnSpawnHeadClientRpc(offset, NetworkManager.LocalClientId);
    }
    [ClientRpc]
    public void OnSpawnHeadClientRpc(Vector3 offset, ulong sender)
    {
        if (sender != NetworkManager.LocalClientId)
            parent.SpawnHead(offset);
    }

    public void OnDespawnHead()
    {
        if (IsServer)
            OnDespawnHeadClientRpc(NetworkManager.LocalClientId);
        else
            OnDespawnHeadServerRpc(NetworkManager.LocalClientId);
    }
    [ServerRpc]
    public void OnDespawnHeadServerRpc(ulong sender)
    {
        parent.DespawnHead(true);
        OnDespawnHeadClientRpc(sender);
    }
    [ClientRpc]
    public void OnDespawnHeadClientRpc(ulong sender)
    {
        if (!NetworkManager.IsServer && sender != NetworkManager.LocalClientId)
            parent.DespawnHead();
    }
}
