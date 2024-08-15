using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(DeskController))]
public class DeskNetwork : NetworkBehaviour
{
    DeskController parent;
    [SerializeField]
    private GameObject onlineIdolPrefab;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        parent = GetComponent<DeskController>();

        if (!IsOwner)
            parent.enabled = false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void TransmitClickServerRpc(ulong sender)
    {
        Debug.Log("Clicked");
        //ConsumeClickClientRpc(sender);
    }
    [ClientRpc]
    private void ConsumeClickClientRpc(ulong sender)
    {
        if (NetworkManager.LocalClientId != sender)
            Debug.Log("Click on client " + sender);
    }
}
