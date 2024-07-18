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
    public GameObject startGameButton;

    private void Start()
    {
        joinInputObj = joinInputField.gameObject;

        createRoomButton.SetActive(true);
        joinRoomButton.SetActive(true);
        joinInputObj.SetActive(true);

        startGameButton.SetActive(false);
    }

    public void PlayOnlineCreate()
    {
        ((MainMenuLobbyController)LobbyController.instance).PlayOnlineCreate();
    }
    public void PlayOnlineJoin()
    {
        ((MainMenuLobbyController)LobbyController.instance).PlayOnlineJoin(joinInputField.text);
    }
    public void Connect()
    {
        ((MainMenuLobbyController)LobbyController.instance).Connect();

        createRoomButton.SetActive(false);
        startGameButton.SetActive(true);
    }

}
