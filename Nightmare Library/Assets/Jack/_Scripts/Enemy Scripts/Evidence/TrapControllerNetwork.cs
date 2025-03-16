using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

using NetVar;

[RequireComponent(typeof(TrapController))]
public class TrapControllerNetwork : HoldableItemNetwork
{
    private TrapController tController;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        tController = GetComponent<TrapController>();

        if (IsOwner)
        {
            OnPickup(true);
        }

        gameObject.SetActive(false);
    }
}
