using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(FootprintController))]
public class FootprintControllerNetwork : NetworkBehaviour
{
    private FootprintController fController;

    public override void OnNetworkSpawn()
    {
        fController = GetComponent<FootprintController>();

        if (IsOwner)
        {
            fController.OnFootprintSpawn += OnFootprintSpawn;
            fController.OnFootprintDespawn += OnFootprintDespawn;

            OnFootprintDespawn(this, EventArgs.Empty);
        }

        gameObject.SetActive(false);
    }

    private void OnFootprintSpawn(object sender, EventArgs e)
    {
        OnFootprintSpawnClientRpc(transform.position);
    }
    private void OnFootprintDespawn(object sender, EventArgs e)
    {
        OnFootprintDespawnClientRpc();
    }

    [ClientRpc]
    public void OnFootprintSpawnClientRpc(Vector3 pos)
    {
        if (!IsOwner)
        {
            transform.position = pos;
            gameObject.SetActive(true);
        }
    }
    [ClientRpc]
    public void OnFootprintDespawnClientRpc()
    {
        if (!IsOwner)
        {
            gameObject.SetActive(false);
        }
    }
}
