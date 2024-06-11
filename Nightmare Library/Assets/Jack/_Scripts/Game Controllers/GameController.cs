using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameController : NetworkBehaviour
{
    private NetworkVariable<ContinuousData> contState;
    float timer = 0;

    private void Awake()
    {
        var permission = NetworkVariableWritePermission.Owner;
        contState = new NetworkVariable<ContinuousData>(writePerm: permission);
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            timer += Time.deltaTime;
            TransmitContinuousState();
        }
        else
        {
            ConsumeContinuousState();
        }
    }

    private void TransmitContinuousState()
    {
        var state = new ContinuousData(timer);

        /// This is not asking if we are a server, but if this
        /// script is set to 'server authoritative' mode.
        /// a better name would have been UsingServerAuthority

        // Needed because we are not able to change info if server has authority
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
