using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class InteractableNetworkState : NetworkBehaviour
{
    private Interactable parent;

    // Network Variables
    private NetworkVariable<ContinuousData> contState;

    private void Awake()
    {
        var permission = NetworkVariableWritePermission.Server;

        contState = new NetworkVariable<ContinuousData>(writePerm: permission);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        parent = GetComponent<Interactable>();

        parent.OnHit += OnHit;
        parent.OnClick += OnClick;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
            TransmitContinuousState();
        else
            ConsumeContinuousState();
    }

    private void TransmitContinuousState()
    {
        var state = new ContinuousData(0);

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
        // Nothing Yet
    }
    [ServerRpc]
    private void TransmitContinuousStateServerRpc(ContinuousData state)
    {
        contState.Value = state;
    }

    private void OnClick(object sender, EventArgs e)
    {
        if (IsOwner)
            ConsumeClickClientRpc(NetworkManager.LocalClientId);
        else
            TransmitClickServerRpc(NetworkManager.LocalClientId);
    }
    private void OnHit(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }


    [ServerRpc(RequireOwnership = false)]
    private void TransmitClickServerRpc(ulong sender)
    {
        ConsumeClickClientRpc(sender);
    }
    [ClientRpc]
    private void ConsumeClickClientRpc(ulong sender)
    {
        if (NetworkManager.LocalClientId != sender)
            Debug.Log("Click on client " + sender);
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
