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
        parent = GetComponent<ScreechHeadController>();
        parent.OnSpawnHead += OnSpawnHead;
        parent.OnDespawnHead += OnDespawnHead;
        parent.OnAttack += OnAttack;
        parent.OnInitialize += OnInitialize;
    }

    public void OnInitialize()
    {
        if (IsServer)
            OnInitializeClientRpc(parent.targetPlayer.GetComponent<PlayerNetwork>().OwnerClientId);
    }
    [ClientRpc]
    private void OnInitializeClientRpc(ulong owner)
    {
        foreach (PlayerController p in PlayerController.playerInstances.Values)
        {
            if (p.GetComponent<PlayerNetwork>().OwnerClientId == owner)
            {
                parent.Initialize(p);
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
    }
    [ClientRpc]
    public void OnDespawnHeadClientRpc(ulong sender)
    {
        if (sender != NetworkManager.LocalClientId)
            parent.DespawnHead();
    }
}
