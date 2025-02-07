using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

using NetVar;

[RequireComponent(typeof(FootprintController))]
public class FootprintNetwork : InteractableNetwork
{
    private FootprintController fController;

    public override void OnNetworkSpawn()
    {
        fController = GetComponent<FootprintController>();

        if (IsOwner)
        {
            OnPickup(true);
        }
        else
        {
            fController.enabled = false;
            enabled = false;
        }

        gameObject.SetActive(false);
    }
}
