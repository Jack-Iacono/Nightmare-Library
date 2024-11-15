using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(WardenSensorController))]
public class WardenSensorNetwork : NetworkBehaviour
{
    private WardenSensorController parent;

    private void Awake()
    {
        parent = GetComponent<WardenSensorController>();

        if (!NetworkConnectionController.IsRunning)
        {
            Destroy(this);
            Destroy(GetComponent<NetworkObject>());
        }
    }
}
