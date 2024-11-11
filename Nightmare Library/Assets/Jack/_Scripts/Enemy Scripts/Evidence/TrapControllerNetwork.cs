using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(TrapController))]
public class TrapControllerNetwork : NetworkBehaviour
{
    private TrapController tController;

    public override void OnNetworkSpawn()
    {
        tController = GetComponent<TrapController>();

        if (IsOwner)
        {
            tController.OnTrapSpawn += OnFootprintSpawn;
            
            OnFootprintDespawn(this, false);
        }

        // Needs to be out here because the clients need to be able to despawn the trap
        tController.OnTrapDespawn += OnFootprintDespawn;

        gameObject.SetActive(false);
    }

    private void OnFootprintSpawn(object sender, bool b)
    {
        if(!b)
            OnTrapSpawnClientRpc(transform.position);
    }
    private void OnFootprintDespawn(object sender, bool b)
    {
        if (!b)
        {
            if (NetworkConnectionController.instance.IsServer)
                OnTrapDespawnClientRpc();
            else
                OnTrapDespawnServerRpc();
        }
    }

    [ClientRpc]
    public void OnTrapSpawnClientRpc(Vector3 pos)
    {
        if (!IsOwner)
        {
            transform.position = pos;
            gameObject.SetActive(true);
        }
    }
    [ClientRpc]
    public void OnTrapDespawnClientRpc()
    {
        tController.Deactivate(true);
    }
    [ServerRpc(RequireOwnership = false)]
    public void OnTrapDespawnServerRpc()
    {
        OnTrapDespawnClientRpc();
        tController.Deactivate(true);
    }
}
