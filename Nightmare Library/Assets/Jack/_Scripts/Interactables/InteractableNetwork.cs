using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class InteractableNetwork : NetworkBehaviour
{
    protected Interactable parent;
    private const float interpolationStrength = 0.1f;

    private bool canUpdateRigidbody = false;
    protected bool ownInteraction = false;
    protected float velocityThreshold = 0.001f;
    protected Vector3 previousPosition = Vector3.zero;

    private int updateTransformFrequency = 3;
    private bool wasUpdating = false;
    private int currentUpdateFrame = 0;

    private bool isUpdatingTransform = false;
    private Vector3 targetPosition = Vector3.zero;
    private Vector3 targetVelocity = Vector3.zero;

    private NetworkVariable<RbData> transformData;
    private NetworkVariable<bool> allEnabled;

    protected virtual void Awake()
    {
        if(!NetworkConnectionController.connectedToLobby)
        {
            Destroy(this);
            Destroy(GetComponent<NetworkObject>());
        }
        else if(NetworkManager.IsServer)
        {
            PrefabHandlerNetwork.AddSpawnedPrefab(GetComponent<NetworkObject>());
        }

        parent = GetComponent<Interactable>();

        var permission = NetworkVariableWritePermission.Server;
        transformData = new NetworkVariable<RbData>(writePerm: permission);
        allEnabled = new NetworkVariable<bool>(writePerm: permission);

        if (IsServer)
            allEnabled.Value = true;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if(parent.allowPlayerClick)
            parent.OnClick += OnClick;
        if (parent.allowPlayerPickup)
        {
            parent.OnPickup += OnPickup;

            // These options have to be available if the player can pick the item up
            parent.OnPlace += OnPlace;
            parent.OnThrow += OnThrow;
        }
        
        if(parent.allowEnemyHysterics)
            parent.OnEnemyInteractHysterics += OnEnemyInteractHysterics;
        if(parent.allowEnemyFlicker)
            parent.OnEnemyInteractFlicker += OnEnemyInteractFlicker;

        parent.OnAllEnabled += OnAllEnabled;

        canUpdateRigidbody = parent.hasRigidBody;

        if (!IsOwner)
        {
            transformData.OnValueChanged += ConsumeTransformData;
            allEnabled.OnValueChanged += ConsumeEnabledData;
        }
    }

    private void FixedUpdate()
    {
        // Check to make sure the network is running to avoid calls going out without being connected and that this is on the server/owner
        if (NetworkConnectionController.IsRunning)
        {
            // Check to see if the object has a rigidbody to update
            if (canUpdateRigidbody)
            {
                if (IsOwner)
                {
                    // ensures the update only runs every few frames
                    if (currentUpdateFrame >= updateTransformFrequency)
                    {
                        // Check if the object is moving enough to pass the velocity check
                        if (Vector3.SqrMagnitude(parent.rb.velocity) > velocityThreshold * velocityThreshold)
                        {
                            // Transmit this object's transform data
                            TransmitTransformData();
                            wasUpdating = true;
                        }
                        // Sends one more update just as the object stops moving
                        else if (wasUpdating)
                        {
                            // Transmit this object's transform data
                            TransmitTransformData();
                            wasUpdating = false;
                        }

                        // Reset the frame counter
                        currentUpdateFrame = 0;
                    }
                    else
                    {
                        // Increment the frame counter
                        currentUpdateFrame++;
                    }
                }
                // Check if the object should be updating it's transform
                else if (isUpdatingTransform && !parent.rb.isKinematic)
                {
                    // Interpolate the position and velocity toward the desired values
                    // Higher Interpolation strength means more accuracy, but also more visual skipping
                    parent.trans.position = Vector3.Slerp(parent.trans.position, targetPosition, interpolationStrength);

                    if (parent.hasRigidBody)
                        parent.rb.velocity = Vector3.Slerp(parent.rb.velocity, targetVelocity, interpolationStrength);

                    // Check if the object is done moving. Turn off the updating to save processing speed
                    if (parent.trans.position == targetPosition)
                    {
                        isUpdatingTransform = false;
                        if(parent.hasRigidBody)
                            parent.rb.velocity = Vector3.zero;
                    }
                }
            }

            
        }
    }

    private void ConsumeEnabledData(bool previousValue, bool newValue)
    {
        parent.EnableAll(newValue);
    }

    #region Transform

    private void TransmitTransformData()
    {
        var state = new RbData
        {
            Position = parent.trans.position,
            Rotation = parent.trans.rotation,
            Velocity = parent.rb.velocity
        };

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
    [ServerRpc(RequireOwnership = false)]
    private void TransmitTransformDataServerRpc(RbData state)
    {
        transformData.Value = state;
    }
    private void ConsumeTransformData(RbData previousValue, RbData newValue)
    {
        // Ensure that the owner does not waste time updating to it's own values
        if (!IsOwner)
        {
            // Warps to the correct starting position
            if(!isUpdatingTransform)
            {
                transform.rotation = transformData.Value.Rotation;
                transform.position = transformData.Value.Position;
                if (parent.hasRigidBody)
                    parent.rb.velocity = transformData.Value.Velocity;

                parent.EnableAll(allEnabled.Value);
            }

            // Tell the object to begin updating it's transform and velocity
            isUpdatingTransform = true;

            // Directly set the rotation to be completly accurate
            transform.rotation = transformData.Value.Rotation;
            
            targetPosition = transformData.Value.Position;
            targetVelocity = newValue.Velocity;
        }
    }

    #endregion

    #region Click
    protected virtual void OnClick(bool fromNetwork = false)
    {
        if (!fromNetwork)
        {
            if (IsOwner)
                ConsumeClickClientRpc(NetworkManager.LocalClientId);
            else
                TransmitClickServerRpc(NetworkManager.LocalClientId);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    protected virtual void TransmitClickServerRpc(ulong sender)
    {
        ConsumeClickClientRpc(sender);
    }
    [ClientRpc]
    protected virtual void ConsumeClickClientRpc(ulong sender)
    {
        if (NetworkManager.LocalClientId != sender)
            Debug.Log("Click on client " + sender);
    }
    #endregion

    #region Pickup

    protected virtual void OnPickup(bool fromNetwork)
    {
        if (!fromNetwork)
        {
            if (IsOwner)
                ConsumePickupClientRpc(NetworkManager.LocalClientId);
            else
            {
                TransmitPickupServerRpc(NetworkManager.LocalClientId);
                isUpdatingTransform = false;
            }
        }
    }
    [ServerRpc(RequireOwnership = false)]
    protected virtual void TransmitPickupServerRpc(ulong sender)
    {
        ConsumePickupClientRpc(sender);
    }
    [ClientRpc]
    protected virtual void ConsumePickupClientRpc(ulong sender)
    {
        if (NetworkManager.LocalClientId != sender)
            parent.Pickup(true);
    }

    #endregion

    #region Place
    protected virtual void OnPlace(bool fromNetwork)
    {
        if (!fromNetwork)
        {
            if (IsOwner)
                ConsumePlaceClientRpc(NetworkManager.LocalClientId, new TransformData(parent.trans.position, parent.trans.rotation));
            else
                TransmitPlaceServerRpc(NetworkManager.LocalClientId, new TransformData(parent.trans.position, parent.trans.rotation));
        }
    }
    [ServerRpc(RequireOwnership = false)]
    protected virtual void TransmitPlaceServerRpc(ulong sender, TransformData data)
    {
        parent.Place(data.Position, data.Rotation, true);
        ConsumePlaceClientRpc(sender, data);
    }
    [ClientRpc]
    protected virtual void ConsumePlaceClientRpc(ulong sender, TransformData data)
    {
        if (!IsServer && NetworkManager.LocalClientId != sender)
            parent.Place(data.Position, data.Rotation, true);
    }
    #endregion

    #region Throw
    protected virtual void OnThrow(Vector3 force, bool fromNetwork)
    {
        if (!fromNetwork)
        {
            // Owner will be server
            if (!IsOwner)
            {
                isUpdatingTransform = false;
                TransmitThrowServerRpc(NetworkManager.LocalClientId, parent.trans.position, force, parent.trans.rotation);
            }
            else
                ConsumeThrowClientRpc(NetworkManager.LocalClientId, parent.trans.position, force, parent.trans.rotation);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    protected virtual void TransmitThrowServerRpc(ulong sender, Vector3 pos, Vector3 force, Quaternion rot)
    {
        parent.Throw(pos, force);

        // Tell the game to update the transform
        currentUpdateFrame = updateTransformFrequency;
    }
    [ClientRpc]
    protected virtual void ConsumeThrowClientRpc(ulong sender, Vector3 pos, Vector3 force, Quaternion rot)
    {
        if (NetworkManager.LocalClientId != sender)
            parent.Throw(pos, force, true);
    }
    #endregion

    #region Enemy Interact Hysterics
    protected virtual void OnEnemyInteractHysterics(bool fromNetwork)
    {
        
    }
    [ServerRpc(RequireOwnership = false)]
    protected virtual void TransmitEnemyInteractHystericsServerRpc(ulong sender)
    {
        ConsumeEnemyInteractHystericsClientRpc(sender);
    }
    [ClientRpc]
    protected virtual void ConsumeEnemyInteractHystericsClientRpc(ulong sender)
    {
        if (NetworkManager.LocalClientId != sender)
            Debug.Log("Click on client " + sender);
    }
    #endregion

    #region Enemy Interact Flicker
    protected virtual void OnEnemyInteractFlicker(bool fromNetwork)
    {
        if (IsServer && !fromNetwork)
            ConsumeEnemyInteractFlickerClientRpc(NetworkManager.LocalClientId);
    }
    [ServerRpc(RequireOwnership = false)]
    protected virtual void TransmitEnemyInteractFlickerServerRpc(ulong sender)
    {
        ConsumeEnemyInteractFlickerClientRpc(sender);
    }
    [ClientRpc]
    protected virtual void ConsumeEnemyInteractFlickerClientRpc(ulong sender)
    {
        if (NetworkManager.LocalClientId != sender)
            parent.EnemyInteractFlicker();
    }
    #endregion

    private void OnAllEnabled(bool enabled)
    {
        if (IsServer)
            allEnabled.Value = enabled;
    }

    protected struct TransformData : INetworkSerializable
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
    protected struct RbData : INetworkSerializable
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

        public RbData(Vector3 pos, Quaternion rot, Vector3 vel)
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
