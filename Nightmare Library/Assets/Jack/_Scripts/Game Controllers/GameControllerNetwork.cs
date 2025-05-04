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
    private static NetworkVariable<int> enemyCount = new NetworkVariable<int>();

    private void Awake()
    {
        if (NetworkConnectionController.CheckNetworkConnected(this))
        {
            if (instance == null)
                instance = this;
            else
                Destroy(this);

            parent = GetComponent<GameController>();

            var permission = NetworkVariableWritePermission.Server;

            contState = new NetworkVariable<ContinuousData>(writePerm: permission);
            gamePaused = new NetworkVariable<bool>(writePerm: permission);
            enemyCount = new NetworkVariable<int>(writePerm: permission);

            PlayerController.OnPlayerAliveChanged += OnPlayerAliveChanged;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        // Changes the gameController data for all versions of this gameobject
        if (!IsOwner)
        {
            parent.enabled = false;
            enemyCount.OnValueChanged += OnEnemyCountValueChanged;
        }
        else
        {
            GameController.OnEnemyCountChanged += OnEnemyCountChanged;
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
    }

    #region Enemy Count

    private void OnEnemyCountChanged(int count)
    {
        enemyCount.Value = count;
    }
    private void OnEnemyCountValueChanged(int previousValue, int newValue)
    {
        if(!IsServer)
            GameController.SetEnemyCount(newValue);
    }

    #endregion

    #region Player Death / Resurrection

    private void OnPlayerAliveChanged(PlayerController player, bool b)
    {
        if(IsServer)
            OnPlayerAliveChangedClientRpc(player.GetComponent<PlayerNetwork>().OwnerClientId, b);
    }
    [ClientRpc]
    private void OnPlayerAliveChangedClientRpc(ulong id, bool b)
    {
        if(id != NetworkManager.LocalClientId)
        {
            VoiceChatController.MutePlayer(id, !b);
        }
    }

    #endregion

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

        if(NetworkConnectionController.connectedToLobby)
            VoiceChatController.UnMuteAll();

        GameController.OnGameEnd -= OnGameEnd;
        PlayerController.OnPlayerAliveChanged -= OnPlayerAliveChanged;
    }
}
