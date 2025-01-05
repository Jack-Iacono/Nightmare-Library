using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace NetVar
{
    public struct TransformData : INetworkSerializable
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

        public TransformData(Vector3 pos, Quaternion rot)
        {
            xPos = pos.x;
            yPos = pos.y;
            zPos = pos.z;

            xRot = rot.eulerAngles.x;
            yRot = rot.eulerAngles.y;
            zRot = rot.eulerAngles.z;
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
    public struct TransformDataRB : INetworkSerializable
    {
        private float xPos, yPos, zPos;
        private float xRot, yRot, zRot;
        private float xVel, yVel, zVel;

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
        internal Vector3 Velocity
        {
            get => new Vector3(xVel, yVel, zVel);
            set
            {
                xVel = value.x;
                yVel = value.y;
                zVel = value.z;
            }
        }

        public TransformDataRB(Vector3 pos, Quaternion rot, Vector3 vel)
        {
            xPos = pos.x;
            yPos = pos.y;
            zPos = pos.z;

            xRot = rot.x;
            yRot = rot.y;
            zRot = rot.z;

            xVel = vel.x;
            yVel = vel.y;
            zVel = vel.z;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref xPos);
            serializer.SerializeValue(ref yPos);
            serializer.SerializeValue(ref zPos);

            serializer.SerializeValue(ref xRot);
            serializer.SerializeValue(ref yRot);
            serializer.SerializeValue(ref zRot);

            serializer.SerializeValue(ref xVel);
            serializer.SerializeValue(ref yVel);
            serializer.SerializeValue(ref zVel);
        }
    }

}
