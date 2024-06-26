using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.MemoryProfiler;
using UnityEngine;

public class MainMenuLobbyController : LobbyController
{

    public async void Connect()
    {
        NetworkConnectionController.connectionType = NetworkConnectionController.ConnectionType.CREATE;

        if (!await StartConnection())
        {
            Debug.LogWarning("Connection Failure");
            await NetworkConnectionController.StopConnection();
        }

        Debug.Log(NetworkConnectionController.joinCode);
    }

    public void PlayOnlineCreate()
    {
        GameController.isNetworkGame = true;
        OnlineSceneController.instance.LoadScene("j_OnlineGame");
    }
    public void PlayOffline()
    {
        GameController.isNetworkGame = false;
        OfflineSceneController.ChangeScene(OfflineSceneController.m_Scene.OFFLINE_GAME);
    }
    public async void PlayOnlineJoin(string joinCode)
    {
        GameController.isNetworkGame = true;

        NetworkConnectionController.connectionType = NetworkConnectionController.ConnectionType.JOIN;
        NetworkConnectionController.joinCode = joinCode;

        if (!await StartConnection())
        {
            Debug.LogWarning("Connection Failure");
            await NetworkConnectionController.StopConnection();
        }
    }

}
