using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameController : NetworkBehaviour
{
    public static GameController instance;

    // Network Variables
    private NetworkVariable<ContinuousData> contState;
    public static NetworkVariable<bool> gamePaused;
    float timer = 0;

    // Events
    private const string onGamePauseEventKey = "GamePauseEventKey";
    public static event EventHandler OnGamePause;

    private static Dictionary<string, EventHandler> eventDict = new Dictionary<string, EventHandler>() 
    {
        { onGamePauseEventKey, OnGamePause }
    };

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);

        var permission = NetworkVariableWritePermission.Owner;

        contState = new NetworkVariable<ContinuousData>(writePerm: permission);
        gamePaused = new NetworkVariable<bool>(writePerm: permission);
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            timer += Time.deltaTime;
            TransmitContinuousState();

            if (Input.GetKeyDown(KeyCode.Q))
            {
                TransmitPauseState();
            }
        }
        else
        {
            ConsumeContinuousState();
        }
    }

    private void TransmitPauseState()
    {
        if (IsOwner)
        {
            gamePaused.Value = !gamePaused.Value;
            EventNotifyClientRpc(onGamePauseEventKey);
        }
        else
        {
            //TransmitContinuousStateServerRpc(state);
        }
    }

    private void TransmitContinuousState()
    {
        var state = new ContinuousData(timer);

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
        timer = contState.Value.timer;
    }

    [ServerRpc]
    private void TransmitContinuousStateServerRpc(ContinuousData state)
    {
        contState.Value = state;
    }

    /// <summary>
    /// Allows the GameController to call events across the network
    /// </summary>
    /// <param name="s">The key of the event that needs to be called</param>
    [ClientRpc]
    private void EventNotifyClientRpc(string s)
    {
        eventDict[s]?.Invoke(this, EventArgs.Empty);
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
