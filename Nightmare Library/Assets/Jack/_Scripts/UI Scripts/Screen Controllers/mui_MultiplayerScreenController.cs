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
    public TMP_Text playerJoinText;


    public override void Initialize(UIController parent)
    {
        base.Initialize(parent);
        joinInputObj = joinInputField.gameObject;

        NetworkConnectionController.OnConnected += OnNetworkConnected;
        LobbyController.OnPlayerListChange += OnPlayerListChange;
    }

    public override void ShowScreen()
    {
        base.ShowScreen();

        if(NetworkConnectionController.IsOnline)
        {
            if(NetworkConnectionController.instance.IsHost)
                startGameButton.SetActive(true);

            playerJoinText.gameObject.SetActive(true);

            createRoomButton.SetActive(false);
            joinRoomButton.SetActive(false);
            joinInputObj.SetActive(false);
        }
        else
        {
            startGameButton.SetActive(false);
            playerJoinText.gameObject.SetActive(false);

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
    public void Connect()
    {
        ((MainMenuLobbyController)LobbyController.instance).Connect();
    }

    private void OnNetworkConnected()
    {
        if (NetworkConnectionController.IsOnline)
        {
            if (NetworkConnectionController.instance.IsHost)
                startGameButton.SetActive(true);

            playerJoinText.gameObject.SetActive(true);

            createRoomButton.SetActive(false);
            joinRoomButton.SetActive(false);
            joinInputObj.SetActive(false);
        }
        else
        {
            startGameButton.SetActive(false);
            playerJoinText.gameObject.SetActive(false);

            createRoomButton.SetActive(true);
            joinRoomButton.SetActive(true);
            joinInputObj.SetActive(true);
        }
    }
    public void OnPlayerListChange()
    {
        playerJoinText.text = string.Empty;

        Dictionary<ulong, LobbyController.PlayerInfo> dict = LobbyController.playerList.GetDictionary();

        foreach(ulong clientId in dict.Keys)
        {
            playerJoinText.text += dict[clientId].username + "\n";
        }
    }

    private void OnDestroy()
    {
        NetworkConnectionController.OnConnected -= OnNetworkConnected;
        LobbyController.OnPlayerListChange -= OnPlayerListChange;
    }
}
