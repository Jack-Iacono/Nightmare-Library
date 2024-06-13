using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

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
        if (instance == null)
            instance = this;
        else
            Destroy(this);

        GameController.OnNetworkGamePause += OnParentPause;

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
        {
            Debug.Log("Test");
            parent.PauseGame(b);
        }
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
}
