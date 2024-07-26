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

    public List<IdolController> SpawnIdols(int count, TaskSpawnIdols idolSpawner)
    {
        List<IdolController> list = new List<IdolController>();
        for (int i = 0; i < count; i++)
        {
            GameObject idol = Instantiate(onlineIdolPrefab, parent.idolGameObjects[i].transform.position, Quaternion.identity, transform);
            idol.name = "Online Idol";
            idol.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
            IdolController iCont = idol.GetComponent<IdolController>();
            iCont.Initialize(idolSpawner);
            list.Add(iCont);

            GameControllerNetwork.instance.spawnedPrefabs.Add(idol);
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
