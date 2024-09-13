using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(MonitorCameraController))]
public class MonitorCameraNetwork : NetworkBehaviour
{
    MonitorCameraController parent;

    private void Awake()
    {
        parent = GetComponent<MonitorCameraController>();
    }

    private void OnPickup(bool fromNetwork = false)
    {
        
    }
    [ServerRpc(RequireOwnership = false)]
    private void OnPickupServerRpc(ulong sender)
    {
        
    }
    [ClientRpc]
    private void OnPickupClientRpc(ulong sender)
    {
        
    }
}
