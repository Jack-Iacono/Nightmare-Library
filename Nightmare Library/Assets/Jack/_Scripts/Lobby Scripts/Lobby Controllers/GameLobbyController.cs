using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine;

public class GameLobbyController : LobbyController
{
    private int connectedPlayers = 0;
    private bool hasSpawned = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    protected override void EntryAction()
    {
        base.EntryAction();

        SpawnOfflinePrefabs();
    }
    protected override void ServerEntryAction()
    {
        base.ServerEntryAction();

        connectedPlayers++;
        CheckAllConnected();
    }
    
    protected override void ClientEntryAction()
    {
        base.ClientEntryAction();

        ClientConnectedServerRpc(NetworkManager.LocalClientId);
    }

    protected override void ConnectVoiceChat()
    {
        VoiceChatController.JoinChannel("Alive", VoiceChatController.ChatType.POSITIONAL);
    }

    #region Spawning

    [ServerRpc(RequireOwnership = false)]
    private void ClientConnectedServerRpc(ulong clientId)
    {
        connectedPlayers++;
        if (!hasSpawned)
            CheckAllConnected();
        else
            SpawnLatePlayers(clientId);
    }

    private void CheckAllConnected()
    {
        // Wait until all players are connected and then load the prefabs
        if (connectedPlayers == NetworkManager.ConnectedClients.Count)
        {
            foreach (ulong id in NetworkManager.ConnectedClients.Keys)
            {
                GameObject pPrefab = PrefabHandler.Instance.InstantiatePrefabOnline(PrefabHandler.Instance.p_Player, new Vector3(-20, 1, 0), Quaternion.identity, id);
                pPrefab.name = "Player " + id;

                spawnedPrefabs.Add(pPrefab);
            }

            for (int i = 0; i < GameController.enemyCount; i++)
            {
                GameObject ePrefab = PrefabHandler.Instance.InstantiatePrefabOnline(PrefabHandler.Instance.e_Enemy, new Vector3(-20, 1, 0), Quaternion.identity);
                ePrefab.name = "Basic Enemy " + instance.OwnerClientId;

                spawnedPrefabs.Add(ePrefab);
            }

            hasSpawned = true;
        }
    }

    // Used for delayed player entry, this should kill them upon spawning in
    private void SpawnLatePlayers(ulong clientId)
    {
        GameObject pPrefab = PrefabHandler.Instance.InstantiatePrefabOnline(PrefabHandler.Instance.p_Player, new Vector3(-20, 1, 0), Quaternion.identity, clientId);
        pPrefab.name = "Player " + clientId;

        spawnedPrefabs.Add(pPrefab);

        pPrefab.GetComponent<PlayerController>().ReceiveAttack();
    }

    private void SpawnOfflinePrefabs()
    {
        PrefabHandler.Instance.InstantiatePrefab(PrefabHandler.Instance.p_Player, new Vector3(-20, 1, 0), Quaternion.identity);
        for (int i = 0; i < GameController.enemyCount; i++)
        {
            PrefabHandler.Instance.InstantiatePrefab(PrefabHandler.Instance.e_Enemy, new Vector3(-20, 1, 0), Quaternion.identity);
        }
    }

    #endregion
}
