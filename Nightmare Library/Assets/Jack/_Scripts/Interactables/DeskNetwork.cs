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
    }

    public List<GameObject> SpawnIdols(int count, TaskSpawnIdols idolSpawner)
    {
        List<GameObject> list = new List<GameObject>();
        for (int i = 0; i < count; i++)
        {
            GameObject idol = Instantiate(onlineIdolPrefab, parent.idolSpawnLocations[i].position, Quaternion.identity, transform);
            idol.name = "Online Idol";
            idol.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
            idol.GetComponent<IdolController>().Initialize(idolSpawner);
            list.Add(idol);
        }
        return list;
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
