using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PreGameLobbyController : LobbyController
{
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

        ClientConnectedServerRpc(NetworkManager.LocalClientId);
    }

    protected override void ConnectVoiceChat()
    {
        VoiceChatController.JoinChannel("Fortnite", VoiceChatController.ChatType.POSITIONAL);
    }

    public void StartGame()
    {
        PrefabHandlerNetwork.Instance.DespawnPrefabs();
        SceneController.LoadScene(SceneController.m_Scene.GAME);
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

    public override void OnDestroy()
    {
        base.OnDestroy();
    }
}
