using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class mui_MultiplayerScreen : ScreenController
{
    [Header("Menu Specific Variables")]
    public TMP_InputField joinInputField;

    private GameObject joinInputObj;
    public GameObject createRoomButton;
    public GameObject joinRoomButton;

    public override void Initialize(UIController parent)
    {
        base.Initialize(parent);
        joinInputObj = joinInputField.gameObject;
    }

    public override void ShowScreen()
    {
        base.ShowScreen();

        if(NetworkConnectionController.IsRunning)
        {
            createRoomButton.SetActive(false);
            joinRoomButton.SetActive(false);
            joinInputObj.SetActive(false);
        }
        else
        {
            createRoomButton.SetActive(true);
            joinRoomButton.SetActive(true);
            joinInputObj.SetActive(true);
        }
    }

    public void PlayOnlineCreate()
    {
        ((MainMenuLobbyController)LobbyController.instance).PlayOnlineCreate();
    }
    public void PlayOnlineJoin()
    {
        ((MainMenuLobbyController)LobbyController.instance).PlayOnlineJoin(joinInputField.text);
    }

    public async void BackButtonPressed()
    {
        await LobbyController.instance.DisconnectFromLobby();
        GoToScreen(0);
    }
}
