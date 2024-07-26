using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class mui_MainScreenController : ScreenController
{
    [Header("Menu Specific Variables")]
    public TMP_InputField joinInputField;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    public async void PlayOffline()
    {
        await NetworkConnectionController.StopConnection();
        ((MainMenuLobbyController)LobbyController.instance).PlayOffline();
    }
}
