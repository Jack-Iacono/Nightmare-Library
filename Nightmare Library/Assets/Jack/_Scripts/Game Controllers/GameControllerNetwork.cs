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

    // Game Info Variables
    private NetworkVariable<NetworkGameInfo> gameInfo;

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
            gameInfo = new NetworkVariable<NetworkGameInfo>(writePerm: permission);

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
            gameInfo.OnValueChanged += OnGameInfoValueChanged;
        }
        else
        {
            GameController.OnEnemyCountChanged += OnEnemyCountChanged;
            GameController.OnGameEnd += OnGameEnd;
            GameController.OnGameInfoChanged += OnGameInfoChanged;
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

    private void OnGameEnd(int endReason)
    {
        if (NetworkManager.IsServer)
            OnGameEndClientRpc(endReason);
    }
    [ClientRpc]
    private void OnGameEndClientRpc(int endReason)
    {
        if (!NetworkManager.IsServer)
        {
            GameController.EndGame(endReason);
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

    #region Game Info

    private void OnGameInfoChanged()
    {
        gameInfo.Value = new NetworkGameInfo(GameController.gameInfo.endReason, GameController.gameInfo.presentEnemies);
    }
    private void OnGameInfoValueChanged(NetworkGameInfo previous,  NetworkGameInfo newValue)
    {
        GameController.gameInfo.SetEndReason(newValue.endReason);
        GameController.gameInfo.presentEnemies = newValue.GetEnemyPresets();
    }
    public struct NetworkGameInfo : INetworkSerializable
    {
        public int endReason;
        public int[] presentEnemies;

        public NetworkGameInfo(int endReason, List<EnemyPreset> presentEnemies)
        {
            this.endReason = endReason;

            int[] e = new int[presentEnemies.Count];

            // Convert the presets into their int formats
            for(int i = 0; i < presentEnemies.Count; i++)
            {
                e[i] = PersistentDataController.Instance.activeEnemyPresets.IndexOf(presentEnemies[i]);
            }

            this.presentEnemies = e;
        }

        public List<EnemyPreset> GetEnemyPresets()
        {
            List<EnemyPreset> p = new List<EnemyPreset>();
            for(int i = 0; i < presentEnemies.Length; i++)
            {
                p.Add(PersistentDataController.Instance.activeEnemyPresets[i]);
            }
            return p;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref endReason);
            Debug.Log(presentEnemies.Length);
            serializer.SerializeValue(ref presentEnemies);
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
        GameController.OnGameInfoChanged -= OnGameInfoChanged;
    }
}
