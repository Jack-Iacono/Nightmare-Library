using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameLobbyController : LobbyController
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    protected override void ServerEntryAction()
    {
        base.ServerEntryAction();
    }
    
    protected override void ClientEntryAction()
    {
        base.ClientEntryAction();
    }

    protected override void ConnectVoiceChat()
    {
        VoiceChatController.JoinChannel("Alive", VoiceChatController.ChatType.POSITIONAL);
    }
}
