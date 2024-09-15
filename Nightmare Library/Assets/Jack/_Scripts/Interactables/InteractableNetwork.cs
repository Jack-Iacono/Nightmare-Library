using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[RequireComponent(typeof(Interactable))]
public class InteractableNetwork : NetworkBehaviour
{
    protected Interactable parent;

    private bool canUpdateRigidbody = false;
    protected bool ownInteraction = false;
    protected float velocityThreshold = 0.05f;
    protected Vector3 previousPosition = Vector3.zero;

    private int updateTransformFrequency = 3;
    private int currentUpdateFrame = 0;

    private bool isUpdatingTransform = false;
    private Vector3 targetTransform = Vector3.zero;

    private NetworkVariable<TransformData> transformData;
    private void Awake()
    {
        parent = GetComponent<Interactable>();

        var permission = NetworkVariableWritePermission.Server;
        transformData = new NetworkVariable<TransformData>(writePerm: permission);
        transformData.OnValueChanged += ConsumeTransformData;
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

        if (IsOwner)
            canUpdateRigidbody = parent.hasRigidBody;
        else if(parent.hasRigidBody)
            parent.rb.isKinematic = true;
    }

    private void FixedUpdate()
    {
        if (NetworkConnectionController.IsRunning)
        {
            if (canUpdateRigidbody)
            {
                // ensures the update only runs every few frames
                if (currentUpdateFrame >= updateTransformFrequency)
                {
                    if (IsOwner)
                    {
                        // Check if the object is moving enough to pass the velocity check
                        if (Vector3.SqrMagnitude(parent.rb.velocity) > velocityThreshold * velocityThreshold)
                        {
                            // Transmit this object's transform data
                            TransmitTransformData();
                        }
                        else
                        {
                            currentUpdateFrame = 0;
                        }
                    }
                }
                else
                {
                    currentUpdateFrame++;
                }

                if (isUpdatingTransform)
                {
                    transform.position = Vector3.Slerp(transform.position, targetTransform, Time.deltaTime * currentUpdateFrame);
                }
            }
        }
    }

    #region Transform

    private void TransmitTransformData()
    {
        var state = new TransformData
        {
            Position = transform.position,
            Rotation = transform.rotation
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
    private void TransmitTransformDataServerRpc(TransformData state)
    {
        transformData.Value = state;
    }
    private void ConsumeTransformData(TransformData previousValue, TransformData newValue)
    {
        if (!IsOwner)
        {
            isUpdatingTransform = true;

            transform.position = Vector3.Slerp(previousValue.Position, newValue.Position, Time.deltaTime * 60);
            transform.rotation = transformData.Value.Rotation;

            targetTransform = newValue.Position + (newValue.Position - previousValue.Position);
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
                TransmitPickupServerRpc(NetworkManager.LocalClientId);
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
                ConsumePlaceClientRpc(NetworkManager.LocalClientId, new TransformData(transform.position, transform.rotation));
            else
                TransmitPlaceServerRpc(NetworkManager.LocalClientId, new TransformData(transform.position, transform.rotation));
        }
    }
    [ServerRpc(RequireOwnership = false)]
    protected virtual void TransmitPlaceServerRpc(ulong sender, TransformData data)
    {
        ConsumePlaceClientRpc(sender, data);
    }
    [ClientRpc]
    protected virtual void ConsumePlaceClientRpc(ulong sender, TransformData data)
    {
        if (NetworkManager.LocalClientId != sender)
            parent.Place(data.Position, data.Rotation, false);
    }
    #endregion

    #region Throw
    protected virtual void OnThrow(Vector3 force, bool fromNetwork)
    {
        if (!fromNetwork)
        {
            // Owner will be server
            if (!IsOwner)
                TransmitThrowServerRpc(NetworkManager.LocalClientId, parent.trans.position, force, parent.trans.rotation);
            else
                ConsumeThrowClientRpc(NetworkManager.LocalClientId);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    protected virtual void TransmitThrowServerRpc(ulong sender, Vector3 pos, Vector3 force, Quaternion rot)
    {
        parent.Throw(pos, force);
    }
    [ClientRpc]
    protected virtual void ConsumeThrowClientRpc(ulong sender)
    {
        if (NetworkManager.LocalClientId != sender)
            parent.EnableAll(true);
    }
    #endregion

    #region Enemy Interact Hysterics
    protected virtual void OnEnemyInteractHysterics(bool fromNetwork)
    {
        ownInteraction = true;
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
        throw new NotImplementedException();
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
            Debug.Log("Click on client " + sender);
    }
    #endregion

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
}
