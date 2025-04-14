using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerNetwork : NetworkBehaviour
{
    public static PlayerNetwork ownerInstance;
    public static BiDict<PlayerController, PlayerNetwork> playerNetworkReference = new BiDict<PlayerController, PlayerNetwork>();

    [SerializeField] private bool _serverAuth;

    private PlayerController playerCont;
    [SerializeField] private GameObject cameraHolder;

    private NetworkVariable<PlayerContinuousNetworkData> playerContinuousState;
    private NetworkVariable<PlayerIntermittentNetworkData> playerIntermittentState;

    private static List<PlayerNetwork> players = new List<PlayerNetwork>();

    private void Awake()
    {
        if (!NetworkConnectionController.connectedToLobby)
        {
            Destroy(this);
            Destroy(GetComponent<NetworkObject>());
        }

        // Can only be written to by server or owner
        var permission = _serverAuth ? NetworkVariableWritePermission.Server : NetworkVariableWritePermission.Owner;
        playerContinuousState = new NetworkVariable<PlayerContinuousNetworkData>(writePerm: permission);
        playerIntermittentState = new NetworkVariable<PlayerIntermittentNetworkData>(writePerm: permission);

        playerCont = GetComponent<PlayerController>();
        PlayerController.OnPlayerAliveChanged += OnPlayerAliveChanged;

        playerNetworkReference.Add(playerCont, this);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
            PlayerController.mainPlayerInstance = playerCont;
        else
            GetComponent<PlayerInteractionController>().enabled = false;

        transform.parent = LobbyController.instance.transform;

        // Changes the player data for all versions of this gameobject
        if (IsOwner)
        {
            ownerInstance = this;
            playerCont.Activate(true);
        }
        else
        {
            playerCont.Activate(false);
        }

        players.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            // May need to update on client as well, not sure yet
            VoiceChatController.UpdatePlayerPosition(gameObject);
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

    #region Player Enable / Disable

    

    #endregion

    #region Player Death

    private void OnPlayerAliveChanged(PlayerController player, bool b)
    {
        // Check that event is about this player and that we are on the server
        if (player == playerCont && IsServer)
        {
            OnPlayerAliveChangedClientRpc(b);
        }
        
    }
    [ClientRpc]
    private void OnPlayerAliveChangedClientRpc(bool b)
    {
        // Do not call again on the owner
        if (!IsServer)
        {
            playerCont.ChangeAliveState(b);
        }
    }

    #endregion

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

        PlayerController.OnPlayerAliveChanged -= OnPlayerAliveChanged;

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
