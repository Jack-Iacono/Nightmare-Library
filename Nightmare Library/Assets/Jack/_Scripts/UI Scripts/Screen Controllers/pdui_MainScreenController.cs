using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class pdui_MainScreenController : ScreenController
{
    [Space(10)]
    [SerializeField]
    private List<TMP_Text> playerNamesTexts = new List<TMP_Text>();
    [SerializeField]
    private TMP_Text joinCodeText;

    private bool displayJoinCode = false;

    public override void Initialize(UIController parent)
    {
        base.Initialize(parent);
        LobbyController.OnPlayerListChange += OnPlayerListChange;

        for(int i = 0; i < playerNamesTexts.Count; i++)
        {
            playerNamesTexts[i].text = "";
        }

        // Functionality in solo play
        if (!NetworkConnectionController.connectedToLobby)
            playerNamesTexts[0].text = "It's you, you're the player";
        else
        {
            OnPlayerListChange();
        }

        UpdateJoinCode();
    }

    public void CopyJoinCode()
    {
        TextEditor te = new TextEditor();
        te.text = NetworkConnectionController.joinCode;
        te.SelectAll();
        te.Copy();
    }

    public void ToggleJoinCodeVisibility()
    {
        displayJoinCode = !displayJoinCode;
        UpdateJoinCode();
    }
    private void UpdateJoinCode()
    {
        joinCodeText.text = displayJoinCode ? NetworkConnectionController.joinCode : "******";
    }

    private void OnPlayerListChange()
    {
        Dictionary<ulong, LobbyController.PlayerInfo> info = LobbyController.playerList.Value.GetDictionary();

        int index = 0;
        foreach(LobbyController.PlayerInfo playerInfo in info.Values)
        {
            if (index < playerNamesTexts.Count)
            {
                playerNamesTexts[index].text = playerInfo.username;
                index++;
            }
            else
                break;
        }
    }
    

    private void OnDestroy()
    {
        LobbyController.OnPlayerListChange -= OnPlayerListChange;
    }
}
