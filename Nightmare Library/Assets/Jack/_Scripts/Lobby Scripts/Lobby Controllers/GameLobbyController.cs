using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        SpawnPlayer();
    }
    protected override void ServerEntryAction()
    {
        base.ServerEntryAction();
        SpawnPlayer(NetworkManager.LocalClientId);
    }

    protected override void ClientEntryAction()
    {
        base.ClientEntryAction();
        //ClientConnectedServerRpc(NetworkManager.LocalClientId);
    }
    protected override void OnClientConnected(ulong obj)
    {
        base.OnClientConnected(obj);

        if(NetworkManager.IsServer)
            SpawnPlayer(obj);
    }

    protected override void ConnectVoiceChat()
    {
        VoiceChatController.JoinChannel("Fortnite", VoiceChatController.ChatType.POSITIONAL);
    }

    public void GoToGame()
    {
        PrefabHandlerNetwork.Instance.DespawnPrefabs();
        SceneController.UnloadScene(SceneController.m_Scene.PREGAME);
        SceneController.LoadScene(SceneController.m_Scene.GAME_SYS);
        SceneController.LoadScene(SceneController.m_Scene.GAME);
    }
    public void GoToPreGame()
    {
        SceneController.UnloadScene(SceneController.m_Scene.GAME_SYS);
        SceneController.UnloadScene(SceneController.m_Scene.GAME);
        SceneController.LoadScene(SceneController.m_Scene.PREGAME);
    }

    #region Spawning

    [ServerRpc(RequireOwnership = false)]
    private void ClientConnectedServerRpc(ulong clientId)
    {
        SpawnPlayer(clientId);
    }
    private void SpawnPlayer()
    {
        GameObject pPrefab = PrefabHandler.Instance.InstantiatePrefab(PrefabHandler.Instance.p_Player, new Vector3(-20, 1, 0), Quaternion.identity);
    }
    private void SpawnPlayer(ulong id)
    {
        GameObject pPrefab = PrefabHandler.Instance.InstantiatePrefabOnline(PrefabHandler.Instance.p_Player, new Vector3(-20, 1, 0), Quaternion.identity, id);
        pPrefab.name = "PreGamePlayer " + id;
    }

    #endregion
}
