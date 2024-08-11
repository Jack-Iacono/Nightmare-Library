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

    public GameObject onlinePlayerPrefab;
    public GameObject onlineEnemyPrefab;

    // Network Variables
    private NetworkVariable<ContinuousData> contState;
    public static NetworkVariable<bool> gamePaused;

    public List<GameObject> spawnedPrefabs = new List<GameObject>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);

        GameController.OnNetworkGamePause += OnParentPause;
        GameController.OnGameEnd += OnGameEnd;
        LobbyController.OnLobbyEnter += OnLobbyEnter;

        var permission = NetworkVariableWritePermission.Owner;

        contState = new NetworkVariable<ContinuousData>(writePerm: permission);
        gamePaused = new NetworkVariable<bool>(writePerm: permission);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        parent = GetComponent<GameController>();

        if (!IsOwner)
            parent.enabled = false;

        // Changes the gameController data for all versions of this gameobject
        if (!IsOwner)
        {
            parent.enabled = false;
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

    private void OnParentPause(object sender, bool e)
    {
        ConsumePauseStateClientRpc(e);
    }
    private void OnGameEnd(object sender, EventArgs e)
    {
        // Unload Spawned Objects
        foreach (GameObject g in spawnedPrefabs)
        {
            g.GetComponent<NetworkObject>().Despawn();
        }

        GameController.OnGameEnd -= OnGameEnd;
        SceneController.LoadScene(SceneController.m_Scene.MAIN_MENU);
    }

    private void OnLobbyEnter(ulong clientId, bool isServer)
    {
        SpawnNetworkObjects();
    }

    public void SpawnNetworkObjects()
    {
        if (NetworkManager.Singleton.IsServer)
            ServerSpawn();
        else
            ClientSpawn();
    }
    private void ServerSpawn()
    {
        // TEMPORARY: Remove the spawn coordinates from this line
        GameObject pPrefab = Instantiate(onlinePlayerPrefab);

        pPrefab.name = "Player " + instance.OwnerClientId;
        pPrefab.GetComponent<NetworkObject>().SpawnWithOwnership(instance.OwnerClientId);

        GameObject ePrefab = Instantiate(onlineEnemyPrefab);

        ePrefab.name = "Basic Enemy " + instance.OwnerClientId;
        ePrefab.GetComponent<NetworkObject>().SpawnWithOwnership(instance.OwnerClientId);

        spawnedPrefabs.Add(pPrefab);
        spawnedPrefabs.Add(ePrefab);
    }
    private void ClientSpawn()
    {
        ClientEntryActionServerRpc(NetworkManager.LocalClientId);
    }
    [ServerRpc(RequireOwnership = false)]
    private void ClientEntryActionServerRpc(ulong clientId)
    {
        GameObject pPrefab = Instantiate(onlinePlayerPrefab);

        pPrefab.name = "Player " + clientId;
        pPrefab.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);

        spawnedPrefabs.Add(pPrefab);
    }

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

    [ClientRpc]
    private void ConsumePauseStateClientRpc(bool b)
    {
        if (NetworkConnectionController.IsRunning && !IsOwner)
            parent.PauseGame(b);
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

    
    public override void OnDestroy()
    {
        // Should never not be this, but just better to check
        if (instance == this)
            instance = null;

        GameController.OnNetworkGamePause -= OnParentPause;
        GameController.OnGameEnd -= OnGameEnd;
        LobbyController.OnLobbyEnter -= OnLobbyEnter;
    }
}
