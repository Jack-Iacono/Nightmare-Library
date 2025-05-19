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

    private NetworkVariable<TransformDataRB> transformData = new NetworkVariable<TransformDataRB>();
    private NetworkVariable<bool> isActive = new NetworkVariable<bool>();
    private NetworkVariable<bool> isHeld = new NetworkVariable<bool>();

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
            transformData.OnValueChanged += ConsumeTransformData;
            isActive.OnValueChanged += ConsumeEnabledData;
            isHeld.OnValueChanged += OnHeldValueChanged;

            // Moves the item to the correct location according to the server
            ConsumeTransformData(transformData.Value, transformData.Value);
        }
        else
        {
            TransmitTransformData();
            isActive.Value = gameObject.activeInHierarchy;
            isHeld.Value = false;
        }
    }

    private void FixedUpdate()
    {
        // Check to make sure the network is running to avoid calls going out without being connected and that this is on the server/owner
        if (NetworkConnectionController.IsRunning)
        {
            if (IsOwner && canUpdateRigidbody && isActive.Value)
            {
                // ensures the update only runs every few frames
                if (currentUpdateFrame >= updateTransformFrequency)
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

                    // Reset the frame counter
                    currentUpdateFrame = 0;
                    previousPosition = transform.position;
                }
                else
                {
                    // Increment the frame counter
                    currentUpdateFrame++;
                }
            }
            else if (!IsOwner && parent.hasRigidBody && parent.rb.velocity == Vector3.zero)
            {
                if (rectifyFrame < rectifyFrequency)
                    rectifyFrame++;
                else
                {
                    //RectifyTransform();
                    rectifyFrame = 0;
                }
            }
        }
    }

    private void ConsumeEnabledData(bool previousValue, bool newValue)
    {
        parent.gameObject.SetActive(newValue);
    }

    #region Transform

    private void TransmitTransformData()
    {
        var state = new TransformDataRB
        {
            Position = parent.trans.position,
            Rotation = parent.trans.rotation,
            Velocity = parent.hasRigidBody ? parent.rb.velocity : Vector3.zero
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
    private void TransmitTransformDataServerRpc(TransformDataRB state)
    {
        transformData.Value = state;
    }
    private void ConsumeTransformData(TransformDataRB previousValue, TransformDataRB newValue)
    {
        // Ensure that the owner does not waste time updating to it's own values
        if (!IsOwner)
        {
            if (Vector3.SqrMagnitude(newValue.Position - transform.position) < transformThreshold * transformThreshold)
            {
                transform.rotation = Quaternion.Lerp(parent.trans.rotation, newValue.Rotation, interpolationStrength);
                parent.trans.position = Vector3.Slerp(parent.trans.position, newValue.Position, interpolationStrength);
                if (parent.hasRigidBody && !parent.rb.isKinematic)
                {
                    parent.rb.velocity = transformData.Value.Velocity;
                }
            }

            rectifyFrame = 0;
        }
    }

    /// <summary>
    /// Forces a sync between the server object and the client object
    /// </summary>
    private void RectifyTransform()
    {
        if (NetworkManager.IsServer)
            RectifyTransformClientRpc();
        else
        {
            parent.trans.position = transformData.Value.Position;
            parent.trans.rotation = transformData.Value.Rotation;
            if (transformData.Value.Velocity != Vector3.zero && parent.hasRigidBody)
            {
                parent.rb.isKinematic = false;
                parent.rb.velocity = transformData.Value.Velocity;
            }
        }
    }
    [ClientRpc]
    private void RectifyTransformClientRpc()
    {
        if (!NetworkManager.IsServer)
            RectifyTransform();
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
            if (IsOwner)
            {
                TransmitTransformData();
                isHeld.Value = true;
                PlaceClientRpc(new TransformData(parent.trans.position, parent.trans.rotation), NetworkManager.LocalClientId);
            }
            else
                TransmitPlaceServerRpc(NetworkManager.LocalClientId, new TransformData(parent.trans.position, parent.trans.rotation));
        }
    }
    [ServerRpc(RequireOwnership = false)]
    protected virtual void TransmitPlaceServerRpc(ulong sender, TransformData data)
    {
        parent.Place(data.Position, data.Rotation, true);

        PlaceClientRpc(data, sender);

        TransmitTransformData();
        isHeld.Value = true;
    }
    [ClientRpc]
    protected virtual void PlaceClientRpc(TransformData data, ulong sender)
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
                TransmitThrowServerRpc(NetworkManager.LocalClientId, parent.trans.position, force, parent.trans.rotation);
            }
            else
                ConsumeThrowClientRpc(NetworkManager.LocalClientId, parent.trans.position, force, parent.trans.rotation);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    protected virtual void TransmitThrowServerRpc(ulong sender, Vector3 pos, Vector3 force, Quaternion rot)
    {
        parent.Throw(pos, force, rot.eulerAngles, true);

        // Tell the game to update the transform
        currentUpdateFrame = updateTransformFrequency;

        ConsumeThrowClientRpc(sender, pos, force, rot);
    }
    [ClientRpc]
    protected virtual void ConsumeThrowClientRpc(ulong sender, Vector3 pos, Vector3 force, Quaternion rot)
    {
        if (NetworkManager.LocalClientId != sender && !NetworkManager.IsServer)
            parent.Throw(pos, force, rot.eulerAngles,true);
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
            OnHeldChangedServerRpc(b);
        else
        {
            isHeld.Value = b;
            OnHeldValueChanged(b, b);
        }
            
    }
    [ServerRpc(RequireOwnership = false)]
    private void OnHeldChangedServerRpc(bool b)
    {
        OnHeldChanged(b);
    }
    private void OnHeldValueChanged(bool previous, bool current)
    {
        parent.SetHeld(current, true);
    }

    #endregion
}
