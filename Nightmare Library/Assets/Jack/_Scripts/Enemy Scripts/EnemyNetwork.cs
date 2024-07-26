using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyNetwork : NetworkBehaviour
{
    [SerializeField] private bool _serverAuth;

    private Enemy enemyController;

    private NetworkVariable<PlayerContinuousNetworkData> contState;
    private NetworkVariable<PlayerIntermittentNetworkData> intState;

    private void Awake()
    {
        // Can only be written to by server or owner
        var permission = _serverAuth ? NetworkVariableWritePermission.Server : NetworkVariableWritePermission.Owner;
        contState = new NetworkVariable<PlayerContinuousNetworkData>(writePerm: permission);
        intState = new NetworkVariable<PlayerIntermittentNetworkData>(writePerm: permission);

        enemyController = GetComponent<Enemy>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        enemyController.Activate(IsOwner);
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
            Rotation = transform.rotation.eulerAngles
        };

        /// This is not asking if we are a server, but if this
        /// script is set to 'server authoritative' mode.
        /// a better name would have been UsingServerAuthority

        // Needed because we are not able to change info if server has authority
        if (IsServer || !_serverAuth)
        {
            contState.Value = state;
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
            intState.Value = state;
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
        contState.Value = state;
    }
    [ServerRpc]
    private void TransmitIntermittentStateServerRpc(PlayerIntermittentNetworkData state)
    {
        intState.Value = state;
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
        transform.position = Vector3.Slerp(transform.position, contState.Value.Position, Time.deltaTime * 60);
        transform.rotation = Quaternion.Euler(contState.Value.Rotation);
    }
    [ClientRpc]
    private void ConsumeIntermittentStateClientRpc()
    {
        // Do nothing yet
    }

    #endregion

    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    #region Data Types

    private struct PlayerContinuousNetworkData : INetworkSerializable
    {
        private float _x, _y, _z;
        private short _yRot;

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

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _x);
            serializer.SerializeValue(ref _y);
            serializer.SerializeValue(ref _z);

            serializer.SerializeValue(ref _yRot);
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
