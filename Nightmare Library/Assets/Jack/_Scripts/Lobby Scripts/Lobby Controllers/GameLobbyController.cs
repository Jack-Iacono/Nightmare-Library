using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameLobbyController : LobbyController
{
    public GameObject playerPrefab;

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkManager.Singleton.IsServer)
            ServerEntryAction();
        else
            ClientEntryAction();
    }

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
}
