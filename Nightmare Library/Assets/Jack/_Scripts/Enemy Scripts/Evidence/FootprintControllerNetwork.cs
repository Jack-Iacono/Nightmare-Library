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
        else
        {
            fController.enabled = false;
            enabled = false;
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

            fController.Activate();
        }
    }
    [ClientRpc]
    public void OnFootprintDespawnClientRpc()
    {
        if (!IsOwner)
        {
            fController.Deactivate();
        }
    }
}
