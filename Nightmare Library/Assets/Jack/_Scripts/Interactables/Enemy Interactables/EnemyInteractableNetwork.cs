using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(EnemyInteractable))]
public class EnemyInteractableNetwork : InteractableNetwork
{
    private NetworkVariable<TransformData> transformData;
    private void Awake()
    {
        var permission = NetworkVariableWritePermission.Server;
        transformData = new NetworkVariable<TransformData>(writePerm: permission);
    }

    void Update()
    {
        if (IsOwner)
        {
            TransmitTransformData();
        }
        else
        {
            ConsumeTransformData();
        }
    }

    private void TransmitTransformData()
    {
        var state = new TransformData
        {
            Position = transform.position,
            Rotation = transform.rotation
        };

        /// This is not asking if we are a server, but if this
        /// script is set to 'server authoritative' mode.
        /// a better name would have been UsingServerAuthority

        // Needed because we are not able to change info if server has authority
        if (IsServer)
        {
            transformData.Value = state;
        }
        else
        {
            TransmitTransformDataServerRpc(state);
        }
    }
    [ServerRpc]
    private void TransmitTransformDataServerRpc(TransformData state)
    {
        transformData.Value = state;
    }
    private void ConsumeTransformData()
    {
        // No interpolation, just using this for testing
        // Movement will not be smooth, but accurate
        transform.position = Vector3.Slerp(transform.position, transformData.Value.Position, Time.deltaTime * 60);
        transform.rotation = transformData.Value.Rotation;
    }

    private struct TransformData : INetworkSerializable
    {
        private float xPos, yPos, zPos;
        private float xRot, yRot, zRot;

        internal Vector3 Position
        {
            get => new Vector3(xPos, yPos, zPos);
            set
            {
                xPos = value.x;
                yPos = value.y;
                zPos = value.z;
            }
        }
        internal Quaternion Rotation
        {
            get => Quaternion.Euler(xRot, yRot, zRot);
            set
            {
                xRot = value.eulerAngles.x;
                yRot = value.eulerAngles.y;
                zRot = value.eulerAngles.z;
            }
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref xPos);
            serializer.SerializeValue(ref yPos);
            serializer.SerializeValue(ref zPos);

            serializer.SerializeValue(ref xRot);
            serializer.SerializeValue(ref yRot);
            serializer.SerializeValue(ref zRot);
        }
    }
}
