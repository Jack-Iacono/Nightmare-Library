using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class GameLobbyController : LobbyController
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
        SceneController.LoadGameScene();
    }
    public void GoToPreGame()
    {
        SceneController.LoadPregameScene();
    }

    #region Spawning

    [ServerRpc(RequireOwnership = false)]
    private void ClientConnectedServerRpc(ulong clientId)
    {
        SpawnPlayer(clientId);
    }
    private void SpawnPlayer()
    {
        SceneController.SetSceneActive(SceneController.m_Scene.UNIVERSAL);
        GameObject pPrefab = PrefabHandler.Instance.InstantiatePrefab(PrefabHandler.Instance.p_Player, new Vector3(-20, 1, 0), Quaternion.identity);
        SceneController.SetMapActive();
    }
    private void SpawnPlayer(ulong id)
    {
        SceneController.SetSceneActive(SceneController.m_Scene.UNIVERSAL);

        GameObject pPrefab = PrefabHandler.Instance.InstantiatePrefabOnline(PrefabHandler.Instance.p_Player, new Vector3(-20, 1, 0), Quaternion.identity, id);
        pPrefab.name = "Player " + id;

        if (GameController.gameStarted)
        {
            PlayerController.playerInstances[pPrefab].ChangeAliveState(false);
        }

        SceneController.SetMapActive();
    }

    #endregion
}
