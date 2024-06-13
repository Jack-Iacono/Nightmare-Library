using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor.Build.Content;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerNetworkState : NetworkBehaviour
{
    public static PlayerNetworkState ownerInstance;

    [SerializeField] private bool _serverAuth;

    private PlayerController playerCont;
    [SerializeField] private GameObject cameraHolder;

    private NetworkVariable<PlayerContinuousNetworkData> playerContinuousState;
    private NetworkVariable<PlayerIntermittentNetworkData> playerIntermittentState;

    private static List<PlayerNetworkState> players = new List<PlayerNetworkState>();

    private void Awake()
    {
        // Can only be written to by server or owner
        var permission = _serverAuth ? NetworkVariableWritePermission.Server : NetworkVariableWritePermission.Owner;
        playerContinuousState = new NetworkVariable<PlayerContinuousNetworkData>(writePerm: permission);
        playerIntermittentState = new NetworkVariable<PlayerIntermittentNetworkData>(writePerm: permission);

        playerCont = GetComponent<PlayerController>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Changes the player data for all versions of this gameobject
        if(IsOwner)
        {
            ownerInstance = this;
            playerCont.ActivatePlayer(true);
        }
        else
        {
            playerCont.ActivatePlayer(false);
        }

        players.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            TransmitContinuousState();
        }
        else
        {
            ConsumeContinuousState();
        }
    }

    public void UpdatePlayerIntermittentState()
    {
        if (IsOwner)
        {
            TransmitIntermittentState();
        }
    }

    #region Server Data Transfers
    private void TransmitContinuousState()
    {
        var state = new PlayerContinuousNetworkData
        {
            Position = transform.position,
            Rotation = transform.rotation.eulerAngles,
            CamRotation = cameraHolder.transform.rotation.eulerAngles
        };

        /// This is not asking if we are a server, but if this
        /// script is set to 'server authoritative' mode.
        /// a better name would have been UsingServerAuthority
        
        // Needed because we are not able to change info if server has authority
        if (IsServer || !_serverAuth)
        {
            playerContinuousState.Value = state;
        }
        else
        {
            TransmitContinuousStateServerRpc(state);
        }
    }
    private void TransmitIntermittentState()
    {
        var state = new PlayerIntermittentNetworkData();

        if (IsServer || !_serverAuth)
        {
            playerIntermittentState.Value = state;
        }
        else
        {
            TransmitIntermittentStateServerRpc(state);
        }

        CallIntermittentDataServerRpc();
    }

    [ServerRpc]
    private void TransmitContinuousStateServerRpc(PlayerContinuousNetworkData state)
    {
        playerContinuousState.Value = state;
    }
    [ServerRpc]
    private void TransmitIntermittentStateServerRpc(PlayerIntermittentNetworkData state)
    {
        playerIntermittentState.Value = state;
    }
    [ServerRpc]
    private void CallIntermittentDataServerRpc()
    {
        ConsumeIntermittentStateClientRpc();
    }

    private void ConsumeContinuousState()
    {
        // No interpolation, just using this for testing
        // Movement will not be smooth, but accurate
        transform.position = Vector3.Slerp(transform.position, playerContinuousState.Value.Position, Time.deltaTime * 60);
        transform.rotation = Quaternion.Euler(playerContinuousState.Value.Rotation);

        cameraHolder.transform.localRotation = Quaternion.Euler(playerContinuousState.Value.CamRotation);
    }
    [ClientRpc]
    private void ConsumeIntermittentStateClientRpc()
    {
        // Do nothing yet
    }

    #endregion

    public override void OnDestroy()
    {
        if (IsOwner)
            players.Clear();
        else
            players.Remove(this);

        base.OnDestroy();
    }

    #region Data Types

    private struct PlayerContinuousNetworkData : INetworkSerializable
    {
        private float _x, _y, _z;
        private short _yRot;
        private short _camXRot;

        internal Vector3 Position
        {
            get => new Vector3(_x, _y, _z);
            set
            {
                _x = value.x;
                _y = value.y;
                _z = value.z;
            }
        }
        internal Vector3 Rotation
        {
            get => new Vector3(0, _yRot, 0);
            set => _yRot = (short)value.y;
        }
        internal Vector3 CamRotation
        {
            get => new Vector3(_camXRot, 0, 0);
            set => _camXRot = (short)value.x;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _x);
            serializer.SerializeValue(ref _y);
            serializer.SerializeValue(ref _z);

            serializer.SerializeValue(ref _yRot);

            serializer.SerializeValue(ref _camXRot);
        }
    }
    private struct PlayerIntermittentNetworkData : INetworkSerializable
    {
        public float health;

        public PlayerIntermittentNetworkData(float health)
        {
            this.health = health;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref health);
        }
    }

    #endregion
}
