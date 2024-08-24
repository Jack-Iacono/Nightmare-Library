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
        parent.OnPickup += OnPickup;
        parent.OnPlace += OnPlace;
    }

    private void OnPickup()
    {
        if (IsOwner)
            OnPickupClientRpc(NetworkManager.LocalClientId);
        else
            OnPickupServerRpc(NetworkManager.LocalClientId);
    }
    [ServerRpc(RequireOwnership = false)]
    private void OnPickupServerRpc(ulong sender)
    {
        OnPickupClientRpc(sender);
    }
    [ClientRpc]
    private void OnPickupClientRpc(ulong sender)
    {
        if (NetworkManager.LocalClientId != sender)
            parent.Pickup(false);
    }

    private void OnPlace(Vector3 pos, Quaternion rot)
    {
        if (IsOwner)
            OnPlaceClientRpc(NetworkManager.LocalClientId, pos, rot);
        else
            OnPlaceServerRpc(NetworkManager.LocalClientId,pos, rot);
    }
    [ServerRpc(RequireOwnership = false)]
    private void OnPlaceServerRpc(ulong sender, Vector3 pos, Quaternion rot)
    {
        OnPlaceClientRpc(sender, pos, rot);
    }
    [ClientRpc]
    private void OnPlaceClientRpc(ulong sender, Vector3 pos, Quaternion rot)
    {
        if (NetworkManager.LocalClientId != sender)
            parent.Place(pos, rot, false);
    }
}
