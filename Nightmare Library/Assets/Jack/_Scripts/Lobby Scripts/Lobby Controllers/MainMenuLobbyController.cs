using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MainMenuLobbyController : LobbyController
{
    public async void PlayOnlineCreate()
    {
        NetworkConnectionController.connectionType = NetworkConnectionController.ConnectionType.CREATE;

        if (!await StartConnection())
        {
            Debug.LogWarning("Connection Failure");
            await NetworkConnectionController.StopConnection();
        }

        // TEMPORARY , copies the join code for me to test stuff
        TextEditor te = new TextEditor();
        te.text = NetworkConnectionController.joinCode;
        te.SelectAll();
        te.Copy();

        GameController.isNetworkGame = true;
        SceneController.LoadScene(SceneController.m_Scene.PREGAME);
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
    public void PlayOffline()
    {
        GameController.isNetworkGame = false;
        SceneController.LoadScene(SceneController.m_Scene.PREGAME);
    }

    public override async void LeaveLobby()
    {
        await DisconnectFromLobby();
        UIController.mainInstance.ChangeToScreen(0);
    }
}
