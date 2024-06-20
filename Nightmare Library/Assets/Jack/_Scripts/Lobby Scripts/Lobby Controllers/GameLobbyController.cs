using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameLobbyController : LobbyController
{
    public GameObject gameControllerPrefab;
    public GameObject playerPrefab;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(NetworkManager.Singleton.IsServer);
        if (NetworkManager.Singleton.IsServer)
            ServerEntryAction();
        else
            ClientEntryAction();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    protected override void ServerEntryAction()
    {
        base.ServerEntryAction();

        GameObject gcPrefab = Instantiate(gameControllerPrefab);

        gcPrefab.name = "GameController " + instance.OwnerClientId;
        gcPrefab.GetComponent<NetworkObject>().SpawnWithOwnership(instance.OwnerClientId);

        GameObject pPrefab = Instantiate(playerPrefab);

        pPrefab.name = "Player " + instance.OwnerClientId;
        pPrefab.GetComponent<NetworkObject>().SpawnWithOwnership(instance.OwnerClientId);

        Debug.Log("Server");
    }
    
    protected override void ClientEntryAction()
    {
        base.ClientEntryAction();

        ClientEntryActionServerRpc(NetworkManager.LocalClientId);

        Debug.Log("Client");
    }
    [ServerRpc(RequireOwnership = false)]
    protected void ClientEntryActionServerRpc(ulong clientId)
    {
        GameObject pPrefab = Instantiate(playerPrefab);

        pPrefab.name = "Player " + clientId;
        pPrefab.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
    }
}
