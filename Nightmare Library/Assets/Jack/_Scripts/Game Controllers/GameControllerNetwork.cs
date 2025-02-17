using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(GameController))]
public class GameControllerNetwork : NetworkBehaviour
{
    public static GameControllerNetwork instance;
    private GameController parent;

    // Network Variables
    private NetworkVariable<ContinuousData> contState;
    public static NetworkVariable<bool> gamePaused;

    private void Awake()
    {
        if (!NetworkConnectionController.connectedToLobby)
        {
            Destroy(this);
            Destroy(GetComponent<NetworkObject>());
        }

        if (instance == null)
            instance = this;
        else
            Destroy(this);

        parent = GetComponent<GameController>();

        var permission = NetworkVariableWritePermission.Owner;

        contState = new NetworkVariable<ContinuousData>(writePerm: permission);
        gamePaused = new NetworkVariable<bool>(writePerm: permission);

        GameController.OnPlayerKilled += OnPlayerKilled;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        // Changes the gameController data for all versions of this gameobject
        if (!IsOwner)
        {
            parent.enabled = false;
        }
        else
        {
            GameController.OnGameEnd += OnGameEnd;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (NetworkManager.IsServer)
            TransmitContinuousState();
        else
            ConsumeContinuousState();

        if (Input.GetKeyDown(KeyCode.P))
        {
            LobbyController.instance.LeaveLobby();
        }
    }

    private void OnPlayerKilled(PlayerController player)
    {
        if(IsOwner)
            OnPlayerKilledClientRpc(player.GetComponent<PlayerNetwork>().OwnerClientId);
    }
    [ClientRpc]
    private void OnPlayerKilledClientRpc(ulong id)
    {
        if(id != NetworkManager.LocalClientId)
        {
            PlayerController p = PlayerController.playerInstances[PlayerNetwork.ownerInstance.gameObject];
            if(p.isAlive)
                VoiceChatController.MutePlayer(id, true);
            else
                VoiceChatController.MutePlayer(id, false);
        }
    }

    #region Game Ending

    private void OnGameEnd()
    {
        if (NetworkManager.IsServer)
            OnGameEndClientRpc();
    }
    [ClientRpc]
    private void OnGameEndClientRpc()
    {
        if (!NetworkManager.IsServer)
        {
            GameController.EndGame();
        }
    }

    #endregion

    #region Continuous Data

    private void TransmitContinuousState()
    {
        var state = new ContinuousData(parent.gameTimer);

        if (NetworkManager.IsServer)
        {
            contState.Value = state;
        }
        else
        {
            TransmitContinuousStateServerRpc(state);
        }
    }
    private void ConsumeContinuousState()
    {
        parent.gameTimer = contState.Value.timer;
    }
    [ServerRpc]
    private void TransmitContinuousStateServerRpc(ContinuousData state)
    {
        contState.Value = state;
    }

    private struct ContinuousData : INetworkSerializable
    {
        public float timer;

        public ContinuousData(float timer)
        {
            this.timer = timer;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref timer);
        }
    }

    #endregion

    public override void OnDestroy()
    {
        // Should never not be this, but just better to check
        if (instance == this)
            instance = null;

        GameController.OnGameEnd -= OnGameEnd;
        GameController.OnPlayerKilled -= OnPlayerKilled;
    }
}
