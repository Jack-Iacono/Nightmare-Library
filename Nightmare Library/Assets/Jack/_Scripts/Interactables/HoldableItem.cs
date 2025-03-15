using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldableItem : MonoBehaviour
{
    public string itemName;

    // Used for easy referencing
    public static Dictionary<GameObject, HoldableItem> instances = new Dictionary<GameObject, HoldableItem>();
    public Transform trans { get; protected set; }

    public enum PlacementType { FLOOR, WALL, CEILING }
    public List<PlacementType> placementTypes = new List<PlacementType>();

    public bool isPhysical { get; private set; } = true;

    /// <summary>
    /// 0: Facing Player 
    /// 1: Facing away from player
    /// </summary>
    [Range(0, 1)]
    public int floorPlacementType = 0;
    /// <summary>
    /// 0: Bottom down, no x or z rotation, facing player
    /// 1: Bottom on wall, top facing player
    /// </summary>
    [Range(0, 1)]
    public int wallPlacementType = 0;
    /// <summary>
    /// True: When the item is placed, disable the rigidbody to fix it to the surface. This will disable if the item is thrown or hit with the enemy hysteric interaction
    /// </summary>
    public bool fixPlacement = true;


    [SerializeField]
    public bool precisePlacement = false;

    // Contains the mesh renderer as well as the default mesh
    protected Dictionary<MeshRenderer, Material> renderMaterialList = new Dictionary<MeshRenderer, Material>();

    protected List<Collider> colliders = new List<Collider>();
    protected Vector3 mainColliderSize = Vector3.zero;

    [NonSerialized]
    public bool hasRigidBody = false;
    [NonSerialized]
    public Rigidbody rb;

    public delegate void OnPickupDelegate(bool fromNetwork = false);
    public event OnPickupDelegate OnPickup;
    public delegate void OnPlaceDelegate(bool fromNetwork = false);
    public event OnPlaceDelegate OnPlace;
    public delegate void OnThrowDelegate(Vector3 force, bool fromNetwork = false);
    public event OnThrowDelegate OnThrow;

    public delegate void OnAllEnabledDelegate(bool enabled);
    public event OnAllEnabledDelegate OnSetPhysical;

    private void Awake()
    {
        HoldableItem.instances.Add(gameObject, this);
    }

    public virtual GameObject Pickup()
    {
        SetPhysical(false);

        if (hasRigidBody)
            rb.isKinematic = true;

        OnPickup?.Invoke();

        return gameObject;
    }
    public virtual void Place(Vector3 pos, Quaternion rot)
    {
        trans.position = pos;
        trans.rotation = rot;

        SetPhysical(true);

        if (fixPlacement && hasRigidBody)
            rb.isKinematic = true;

        OnPlace?.Invoke();
    }
    public virtual void Throw(Vector3 pos, Vector3 force)
    {
        trans.position = pos;
        SetPhysical(true);

        if (hasRigidBody)
        {
            rb.isKinematic = false;
            rb.AddForce(force, ForceMode.Impulse);
        }

    }

    public void SetPhysical(bool b)
    {
        EnableColliders(b);
        EnableMesh(b);
        gameObject.SetActive(b);

        if (b)
            ResetMeshMaterial();

        OnSetPhysical?.Invoke(b);
    }
    public void EnableColliders(bool b)
    {
        foreach (Collider c in colliders)
        {
            c.enabled = b;
        }
        if (!b && hasRigidBody)
            rb.isKinematic = true;

        isPhysical = b;
    }

    public void EnableMesh(bool b)
    {
        foreach (MeshRenderer r in renderMaterialList.Keys)
        {
            r.enabled = b;
        }
    }
    public void SetMeshMaterial(Material mat)
    {
        foreach (MeshRenderer r in renderMaterialList.Keys)
        {
            r.material = mat;
        }
    }
    public void ResetMeshMaterial()
    {
        foreach (MeshRenderer r in renderMaterialList.Keys)
        {
            r.material = renderMaterialList[r];
        }
    }

    public Vector3 GetColliderSize()
    {
        return mainColliderSize;
    }

    private void OnDestroy()
    {
        HoldableItem.instances.Remove(gameObject);
    }
}
