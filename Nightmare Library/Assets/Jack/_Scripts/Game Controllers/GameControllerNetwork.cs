using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

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

    private void Awake()
    {
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

        // Changes the player data for all versions of this gameobject
        if (!IsOwner)
        {
            parent.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
            TransmitContinuousState();
        else
            ConsumeContinuousState();
    }

    private void OnParentPause(object sender, bool e)
    {
        ConsumePauseStateClientRpc(e);
    }

    private void OnLobbyEnter(ulong clientId, bool isServer)
    {
        SpawnNetworkObjects();
    }

    public void SpawnNetworkObjects()
    {
        Debug.Log("Spawning Players");
        if (NetworkManager.Singleton.IsServer)
            ServerSpawn();
        else
            ClientSpawn();
    }
    private void ServerSpawn()
    {
        GameObject pPrefab = Instantiate(onlinePlayerPrefab);

        pPrefab.name = "Player " + instance.OwnerClientId;
        pPrefab.GetComponent<NetworkObject>().SpawnWithOwnership(instance.OwnerClientId);
        pPrefab.transform.position += Vector3.up * 10;

        GameObject ePrefab = Instantiate(onlineEnemyPrefab);

        ePrefab.name = "Basic Enemy " + instance.OwnerClientId;
        ePrefab.GetComponent<NetworkObject>().SpawnWithOwnership(instance.OwnerClientId);
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
    }

    private void TransmitContinuousState()
    {
        var state = new ContinuousData(parent.timer);

        if (IsOwner)
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
        parent.timer = contState.Value.timer;
    }
    [ServerRpc]
    private void TransmitContinuousStateServerRpc(ContinuousData state)
    {
        contState.Value = state;
    }

    [ClientRpc]
    private void ConsumePauseStateClientRpc(bool b)
    {
        if (!IsOwner)
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
        LobbyController.OnLobbyEnter -= OnLobbyEnter;
    }
}
