using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameLobbyController : LobbyController
{
    protected override void Awake()
    {
        base.Awake();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();  

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
