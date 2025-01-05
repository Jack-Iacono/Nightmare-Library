using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Unity.Burst.CompilerServices;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyNetwork : NetworkBehaviour
{
    [SerializeField] private bool _serverAuth;

    public Enemy parent { get; private set; }

    private NetworkVariable<PlayerContinuousNetworkData> contState;
    private NetworkVariable<PlayerIntermittentNetworkData> intState;

    private void Awake()
    {
        if (!NetworkConnectionController.connectedToLobby)
        {
            Destroy(this);
            Destroy(GetComponent<NetworkObject>());
        }

        // Can only be written to by server or owner
        var permission = _serverAuth ? NetworkVariableWritePermission.Server : NetworkVariableWritePermission.Owner;
        contState = new NetworkVariable<PlayerContinuousNetworkData>(writePerm: permission);
        intState = new NetworkVariable<PlayerIntermittentNetworkData>(writePerm: permission);

        parent = GetComponent<Enemy>();
        parent.OnInitialize += OnInitialize;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        parent.Activate(IsOwner);

        if (IsOwner)
        {
            parent.OnPlaySound += OnPlaySound;
            parent.OnHallucination += OnHallucination;
            parent.OnLightFlicker += OnLightFlicker;

            // Possible Optimize
            parent.OnSpawnFootprint += OnSpawnFootprint;
            parent.OnSpawnTrap += OnSpawnTrap;
        }
    }
    private void OnInitialize()
    {
        if (IsServer)
        {
            OnInitializeClientRpc(parent.presets.IndexOf(parent.enemyType), (int)parent.aAttack, (int)parent.pAttack);
        }
    }
    [ClientRpc]
    private void OnInitializeClientRpc(int typeIndex, int activeAttackIndex, int passiveAttackIndex)
    {
        parent.enemyType = parent.presets[typeIndex];
        parent.aAttack = (EnemyPreset.aAttackEnum)activeAttackIndex;
        parent.pAttack = (EnemyPreset.pAttackEnum)passiveAttackIndex;
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

    #region Evidence Methods

    public void OnPlaySound(object sender, string sound)
    {
        PlaySoundClientRpc(sound);
    }
    [ClientRpc]
    private void PlaySoundClientRpc(string sound)
    {
        if (!NetworkManager.IsServer)
            parent.PlaySound(sound);
    }

    public void OnSpawnFootprint(Vector3 pos)
    {
        if (NetworkConnectionController.HasAuthority)
        {
            var print = parent.objPool.GetObject(PrefabHandler.Instance.e_EvidenceFootprint);

            print.GetComponent<FootprintController>().Place(pos, Quaternion.identity);
            print.SetActive(true);
        }
    }
    public void OnSpawnTrap(Vector3 pos)
    {
        if (NetworkConnectionController.HasAuthority)
        {
            var print = parent.objPool.GetObject(PrefabHandler.Instance.e_EvidenceTrap);

            print.GetComponent<TrapController>().Place(pos, print.transform.rotation);
            print.SetActive(true);
        }
    }

    private void OnHallucination(object sender, bool b)
    {
        if (NetworkConnectionController.HasAuthority)
        {
            OnHallucinationClientRpc(b);
        }
    }
    [ClientRpc]
    private void OnHallucinationClientRpc(bool b)
    {
        if (!NetworkManager.IsServer)
            parent.SetHallucinating(b, false);
    }

    private void OnLightFlicker(object sender, EventArgs e)
    {
        if (NetworkConnectionController.HasAuthority)
        {
            OnLightFlickerClientRpc();
        }
    }
    [ClientRpc]
    private void OnLightFlickerClientRpc()
    {
        parent.FlickerLights(false);
    }

    #endregion

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
