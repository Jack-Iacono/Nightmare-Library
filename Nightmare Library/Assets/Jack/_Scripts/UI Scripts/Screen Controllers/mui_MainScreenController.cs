using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class mui_MainScreenController : ScreenController
{
    [Header("Menu Specific Variables")]
    public TMP_InputField joinInputField;

    public void PlayOnlineCreate()
    {
        ((MainMenuLobbyController)LobbyController.instance).PlayOnlineCreate();
    }
    public void PlayOffline()
    {
        ((MainMenuLobbyController)LobbyController.instance).PlayOffline();
    }
    public void PlayOnlineJoin()
    {
        ((MainMenuLobbyController)LobbyController.instance).PlayOnlineJoin(joinInputField.text);
    }
    public void Connect()
    {
        ((MainMenuLobbyController)LobbyController.instance).Connect();
    }
}
