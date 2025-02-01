using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
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

        TextEditor te = new TextEditor();
        te.text = NetworkConnectionController.joinCode;
        te.SelectAll();
        te.Copy();
    }

    public override async void LeaveLobby()
    {
        await DisconnectFromLobby();
        UIController.mainInstance.ChangeToScreen(0);
    }

    public void PlayOnlineCreate()
    {
        GameController.isNetworkGame = true;
        SceneController.LoadScene(SceneController.m_Scene.GAME);
    }
    public void PlayOffline()
    {
        GameController.isNetworkGame = false;
        SceneController.LoadScene(SceneController.m_Scene.GAME);
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
