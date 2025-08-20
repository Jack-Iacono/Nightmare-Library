using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using NetVar;

[RequireComponent(typeof(HoldableItem))]
public class HoldableItemNetwork : NetworkBehaviour
{
    // Used to easily reference specific holdable objects over the network
    public static BiDict<HoldableItem, ulong> idLink = new BiDict<HoldableItem, ulong>();

    private HoldableItem parent;
    private bool canUpdateRigidbody = false;
    protected bool ownInteraction = false;

    protected float movementThreshold = 0.001f;
    protected Vector3 previousPosition = Vector3.zero;

    protected const float transformThreshold = 0.5f;
    private const float interpolationStrength = 0.6f;

    private int rectifyFrequency = 60;
    private int rectifyFrame = 0;

    private int updateTransformFrequency = 1;

    private bool wasUpdating = false;
    private int currentUpdateFrame = 0;

    private NetworkVariable<TransformData> transformData = new NetworkVariable<TransformData>();
    private NetworkVariable<bool> isActive = new NetworkVariable<bool>();
    private NetworkVariable<HeldData> isHeld = new NetworkVariable<HeldData>();

    protected virtual void Awake()
    {
        if (NetworkConnectionController.CheckNetworkConnected(this))
        {
            if (NetworkManager.IsServer)
            {
                PrefabHandlerNetwork.AddSpawnedPrefab(GetComponent<NetworkObject>());
            }

            parent = GetComponent<HoldableItem>();
            previousPosition = transform.position;
        }
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        idLink.Add(parent, NetworkObjectId);

        parent.OnPickup += OnPickup;
        parent.OnPlace += OnPlace;
        parent.OnThrow += OnThrow;

        parent.OnActiveChanged += OnActiveChanged;
        parent.OnHeldChanged += OnHeldChanged;

        canUpdateRigidbody = parent.hasRigidBody;

        if (!IsOwner)
        {
            isActive.OnValueChanged += OnActiveValueChanged;
            isHeld.OnValueChanged += OnHeldValueChanged;

            ConsumeTransformData();
        }
        else
        {
            isActive.Value = gameObject.activeInHierarchy;
            isHeld.Value = new HeldData(false, 0);

            TransmitTransformData();
        }
    }

    private void FixedUpdate()
    {
        // Check to make sure the network is running to avoid calls going out without being connected and that this is on the server/owner
        if (NetworkConnectionController.IsRunning)
        {
            if (!isHeld.Value.isHeld)
            {
                // Check to make sure that this object has a rigid body, is active and is being run on the server
                if (NetworkManager.IsServer && canUpdateRigidbody && isActive.Value)
                {
                    // Check if the object is moving enough to pass the velocity check
                    if (Vector3.SqrMagnitude(previousPosition - transform.position) > movementThreshold * movementThreshold)
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
                        RectifyTransform();
                        wasUpdating = false;
                    }

                    previousPosition = transform.position;
                }
                else if (!NetworkManager.IsServer)
                {
                    if (rectifyFrame < rectifyFrequency)
                        rectifyFrame++;
                    else
                    {
                        RectifyTransform();
                        rectifyFrame = 0;
                    }
                }
            }
            // Only run if the item is being held by this client
            else if (isHeld.Value.isHeld && NetworkManager.LocalClientId == isHeld.Value.holderID)
            {
                TransmitTransformData();
            }
        }
    }

    #region Transform

    private void TransmitTransformData()
    {
        var state = new TransformData
        {
            Position = parent.trans.position,
            Rotation = parent.trans.rotation,
            Velocity = parent.hasRigidBody ? parent.rb.velocity : Vector3.zero,
            isKinematic = parent.GetKinematic()
        };

        // just a safety net in case the client somehow is able to call this method
        if (NetworkManager.IsServer)
        {
            transformData.Value = state;
            TransformDataUpdateClientRpc();
        }
        else
        {
            TransformDataUpdateServerRpc(state);
        }
    }
    [ClientRpc]
    private void TransformDataUpdateClientRpc()
    {
        if (!NetworkManager.IsServer && isHeld.Value.holderID != NetworkManager.LocalClientId)
        {
            ConsumeTransformData();
        }

        rectifyFrame = 0;
    }
    [ServerRpc(RequireOwnership = false)]
    private void TransformDataUpdateServerRpc(TransformData data)
    {
        transformData.Value = data;
        ConsumeTransformData();
        TransformDataUpdateClientRpc();
    }

    private void ConsumeTransformData()
    {
        transform.rotation = Quaternion.Lerp(parent.trans.rotation, transformData.Value.Rotation, interpolationStrength);
        parent.trans.position = Vector3.Slerp(parent.trans.position, transformData.Value.Position, interpolationStrength);
        if (parent.hasRigidBody && !parent.rb.isKinematic)
        {
            parent.rb.velocity = transformData.Value.Velocity;
            parent.rb.isKinematic = transformData.Value.isKinematic;
        }
    }

    /// <summary>
    /// Forces the clients to set object position directly to the current transform of the server with no slerp
    /// </summary>
    private void RectifyTransform()
    {
        parent.trans.position = transformData.Value.Position;
        parent.trans.rotation = transformData.Value.Rotation;
        if (parent.hasRigidBody && transformData.Value.Velocity != Vector3.zero)
        {
            parent.rb.isKinematic = transformData.Value.isKinematic;
            parent.rb.velocity = transformData.Value.Velocity;
        }

        rectifyFrame = 0;
    }

    #endregion

    #region Pickup

    protected virtual void OnPickup(bool fromNetwork = false)
    {
        if (!fromNetwork)
        {
            if (IsOwner)
                ConsumePickupClientRpc(NetworkManager.LocalClientId);
            else
            {
                TransmitPickupServerRpc(NetworkManager.LocalClientId);
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
            if (NetworkManager.IsServer)
            {
                TransmitTransformData();
                PlaceClientRpc(new TransformData(parent.trans.position, parent.trans.rotation, Vector3.zero, parent.GetKinematic()), NetworkManager.LocalClientId);
            }
            else
            {
                TransmitPlaceServerRpc(NetworkManager.LocalClientId, new TransformData(parent.trans.position, parent.trans.rotation, Vector3.zero, parent.GetKinematic()));
            }
        }
    }
    [ServerRpc(RequireOwnership = false)]
    protected virtual void TransmitPlaceServerRpc(ulong sender, TransformData data)
    {
        parent.Place(data.Position, data.Rotation, true);
        TransmitTransformData();
        PlaceClientRpc(data, sender);
    }
    [ClientRpc]
    protected virtual void PlaceClientRpc(TransformData data, ulong sender)
    {
        if (!IsServer && NetworkManager.LocalClientId != sender)
            parent.Place(data.Position, data.Rotation, true);
    }
    #endregion

    #region Throw

    protected virtual void OnThrow(Vector3 force, Vector3 rotForce, bool fromNetwork)
    {
        if (!fromNetwork)
        {
            // Owner will be server
            if (!IsOwner)
            {
                TransmitThrowServerRpc(NetworkManager.LocalClientId, parent.trans.position, parent.trans.rotation, force, rotForce);
            }
            else
                ConsumeThrowClientRpc(NetworkManager.LocalClientId, parent.trans.position, parent.trans.rotation, force, rotForce);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    protected virtual void TransmitThrowServerRpc(ulong sender, Vector3 pos, Quaternion rot, Vector3 force, Vector3 rotForce)
    {
        parent.Throw(pos, rot.eulerAngles, force, Vector3.zero,true);

        // Tell the game to update the transform
        currentUpdateFrame = updateTransformFrequency;

        ConsumeThrowClientRpc(sender, pos, rot, force, rotForce);
    }
    [ClientRpc]
    protected virtual void ConsumeThrowClientRpc(ulong sender, Vector3 pos, Quaternion rot, Vector3 force, Vector3 rotForce)
    {
        if (NetworkManager.LocalClientId != sender && !NetworkManager.IsServer)
            parent.Throw(pos, rot.eulerAngles, force, rotForce, true);
    }

    #endregion

    #region Active / Colliders

    private void OnActiveChanged(bool b)
    {
        if(!NetworkManager.IsServer)
            OnActiveChangedServerRpc(b);
        else
        {
            isActive.Value = b;
            OnActiveValueChanged(b, b);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void OnActiveChangedServerRpc(bool b)
    {
        OnActiveChanged(b);
    }
    private void OnActiveValueChanged(bool previous, bool current)
    {
        parent.SetActive(current, true);
    }

    private void OnHeldChanged(bool b)
    {
        if (!NetworkManager.IsServer)
            OnHeldChangedServerRpc(new HeldData(b, NetworkManager.LocalClientId));
        else
        {
            isHeld.Value = new HeldData(b, NetworkManager.LocalClientId);
            OnHeldValueChanged(isHeld.Value, isHeld.Value);
        }
            
    }
    [ServerRpc(RequireOwnership = false)]
    private void OnHeldChangedServerRpc(HeldData data)
    {
        isHeld.Value = data;
        OnHeldValueChanged(data, data);
    }
    private void OnHeldValueChanged(HeldData previous, HeldData current)
    {
        parent.SetHeld(current.isHeld, true);
    }

    #endregion

    #region Classes and Structs

    public struct TransformData : INetworkSerializable
    {
        private float xPos, yPos, zPos;
        private float xRot, yRot, zRot;

        private float xVel, yVel, zVel;
        public bool isKinematic;

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

        public TransformData(Vector3 pos, Quaternion rot, Vector3 vel, bool isKinematic)
        {
            xPos = pos.x;
            yPos = pos.y;
            zPos = pos.z;

            xRot = rot.eulerAngles.x;
            yRot = rot.eulerAngles.y;
            zRot = rot.eulerAngles.z;

            xVel = vel.x;
            yVel = vel.y;
            zVel = vel.z;

            this.isKinematic = isKinematic;
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

            serializer.SerializeValue(ref isKinematic);
        }
    }
    public struct HeldData : INetworkSerializable
    {
        public bool isHeld;
        public ulong holderID;

        public HeldData(bool isHeld, ulong holderID)
        {
            this.isHeld = isHeld;
            this.holderID = holderID;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref isHeld);
            serializer.SerializeValue(ref holderID);
        }
    }

    #endregion
}
