using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.PackageManager;
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

    public List<GameObject> spawnedPrefabs = new List<GameObject>();

    private int connectedPlayers = 0;
    private bool hasSpawned = false;

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

        GameController.OnNetworkGamePause += OnParentPause;
        LobbyController.OnLobbyEnter += OnLobbyEnter;

        var permission = NetworkVariableWritePermission.Owner;

        contState = new NetworkVariable<ContinuousData>(writePerm: permission);
        gamePaused = new NetworkVariable<bool>(writePerm: permission);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        parent = GetComponent<GameController>();

        // Changes the gameController data for all versions of this gameobject
        if (!IsOwner)
        {
            parent.enabled = false;
        }
        else
        {
            GameController.OnGameEnd += OnGameEnd;
            GameController.OnReturnToMenu += OnReturnToMenu;
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

    private void OnParentPause(object sender, bool e)
    {
        OnParentPauseClientRpc(e);
    }
    [ClientRpc]
    private void OnParentPauseClientRpc(bool b)
    {
        if (NetworkConnectionController.IsRunning && !IsOwner)
            parent.PauseGame(b);
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
            parent.EndGame();
        }
    }

    private void OnReturnToMenu()
    {
        // Unload Spawned Objects
        PrefabHandlerNetwork.Instance.DespawnPrefabs();

        GameController.OnReturnToMenu -= OnReturnToMenu;
        GameController.OnGameEnd -= OnGameEnd;

        SceneController.LoadScene(SceneController.m_Scene.MAIN_MENU);
    }

    #endregion

    #region Connecting to Game

    private void OnLobbyEnter(ulong clientId, bool isServer)
    {
        if (NetworkManager.Singleton.IsServer)
            ServerConnected();
        else
            ClientConnected();
    }

    private void ServerConnected()
    {
        connectedPlayers++;
        CheckAllConnected();
    }
    private void ClientConnected()
    {
        ClientConnectedServerRpc(NetworkManager.LocalClientId);
    }
    [ServerRpc(RequireOwnership = false)]
    private void ClientConnectedServerRpc(ulong clientId)
    {
        connectedPlayers++;
        if (!hasSpawned)
            CheckAllConnected();
        else
            SpawnPlayer(clientId);
    }

    private void CheckAllConnected()
    {
        // Wait until all players are connected and then load the prefabs
        if(connectedPlayers == NetworkManager.ConnectedClients.Count)
        {
            foreach(ulong id in NetworkManager.ConnectedClients.Keys)
            {
                GameObject pPrefab = PrefabHandler.Instance.InstantiatePrefabOnline(PrefabHandler.Instance.p_Player, new Vector3(-20, 1, 0), Quaternion.identity, id);
                pPrefab.name = "Player " + id;

                spawnedPrefabs.Add(pPrefab);
            }

            GameObject ePrefab = PrefabHandler.Instance.InstantiatePrefabOnline(PrefabHandler.Instance.e_Enemy, new Vector3(-20, 1, 0), Quaternion.identity);
            ePrefab.name = "Basic Enemy " + instance.OwnerClientId;

            spawnedPrefabs.Add(ePrefab);

            hasSpawned = true;
        }
    }

    // Used for delayed player entry, this should kill them upon spawning in
    private void SpawnPlayer(ulong clientId)
    {
        GameObject pPrefab = PrefabHandler.Instance.InstantiatePrefabOnline(PrefabHandler.Instance.p_Player, new Vector3(-20, 1, 0), Quaternion.identity, clientId);
        pPrefab.name = "Player " + clientId;

        spawnedPrefabs.Add(pPrefab);

        pPrefab.GetComponent<PlayerController>().ReceiveAttack();
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

        GameController.OnNetworkGamePause -= OnParentPause;
        GameController.OnGameEnd -= OnGameEnd;
        GameController.OnReturnToMenu -= OnReturnToMenu;
        LobbyController.OnLobbyEnter -= OnLobbyEnter;
    }
}
