using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MainMenuLobbyController : LobbyController
{
    public async void PlayOnlineCreate()
    {
        await AuthenticationController.SignInAnonymously();

        NetworkConnectionController.connectionType = NetworkConnectionController.ConnectionType.CREATE;

        if (!await StartConnection())
        {
            Debug.LogWarning("Connection Failure");
            await NetworkConnectionController.StopConnection();
        }

        GameController.isNetworkGame = true;

        SceneController.LoadPregameScene();
    }
    public async void PlayOnlineJoin(string joinCode)
    {
        GameController.isNetworkGame = true;

        await AuthenticationController.SignInAnonymously();

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
        SceneController.LoadPregameScene();
    }

    public override async void LeaveLobby()
    {
        await DisconnectFromLobby();
        UIController.mainInstance.ChangeToScreen(0);
    }
}
