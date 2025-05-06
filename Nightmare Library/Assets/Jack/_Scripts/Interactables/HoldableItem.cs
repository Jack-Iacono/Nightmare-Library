using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldableItem : MonoBehaviour, IEnemyHystericObject
{
    public string itemName;

    // Used for easy referencing
    public static Dictionary<GameObject, HoldableItem> instances = new Dictionary<GameObject, HoldableItem>();
    public Transform trans { get; protected set; }

    [SerializeField]
    private GameObject gameobjectOverride = null;
    [SerializeField]
    public MeshFilter mainMeshFilter = null;
    [NonSerialized]
    public Material mainMaterial = null;

    public enum PlacementType { FLOOR, WALL, CEILING }
    public List<PlacementType> placementTypes = new List<PlacementType>();

    public bool isPhysical { get; private set; } = true;

    /// <summary>
    /// 0: Facing Player 
    /// 1: Facing away from player
    /// </summary>
    [Range(0, 1), Tooltip("0: Facing Player , facing player\n1: Facing away from player") ]
    public int floorPlacementType = 0;
    /// <summary>
    /// 0: Bottom down, no x or z rotation, facing player
    /// 1: Bottom on wall, top facing player
    /// </summary>
    [Range(0, 1), Tooltip("0: Bottom down, no x or z rotation, facing player\n1: Bottom on wall, top facing player")]
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

    protected virtual void Awake()
    {
        if(mainMeshFilter != null)
            mainMaterial = mainMeshFilter.gameObject.GetComponent<MeshRenderer>().material;

        // Add this object to the dictionary for easy referncing from other scripts via the gameObject
        if (gameobjectOverride == null)
        {
            HoldableItem.instances.Add(gameObject, this);
            IEnemyHystericObject.instances.Add(gameObject, this);
        }
        else
        {
            HoldableItem.instances.Add(gameobjectOverride, this);
            IEnemyHystericObject.instances.Add(gameobjectOverride, this);
        }

        // Find the renderers present on this object for use with placement
        foreach (MeshRenderer r in GetComponentsInChildren<MeshRenderer>())
        {
            renderMaterialList.Add(r, r.material);
        }

        // Find all the colliders associated with this object
        foreach (Collider col in GetComponentsInChildren<Collider>())
        {
            colliders.Add(col);
        }

        // Get the size of the main collider for the object
        if (colliders.Count > 0)
            mainColliderSize = colliders[0].bounds.size;
        else
            mainColliderSize = Vector3.zero;

        hasRigidBody = TryGetComponent(out rb);
        trans = transform;
    }

    public virtual GameObject Pickup(bool fromNetwork = false)
    {
        // Make the object intangible
        gameObject.SetActive(false);

        // If the object has a rigid body, stop it from moving
        if (hasRigidBody)
            rb.isKinematic = true;

        // Alert that this object has been picked up (used mostly for network decoupling)
        OnPickup?.Invoke(fromNetwork);

        return gameObject;
    }
    public virtual void Place(Vector3 pos, Quaternion rot, bool fromNetwork = false)
    {
        // Place the object at the desired position
        trans.position = pos;
        trans.rotation = rot;

        // Make the object tangible
        gameObject.SetActive(true);

        // make kinematic if the object has the fixPlacement modifier
        if (fixPlacement && hasRigidBody)
            rb.isKinematic = true;

        // Alert that this object has been placed
        OnPlace?.Invoke(fromNetwork);
    }
    public virtual void Throw(Vector3 pos, Vector3 force, Vector3 rot, bool fromNetwork = false)
    {
        trans.position = pos;
        trans.rotation = Quaternion.Euler(rot);
        gameObject.SetActive(true);

        if (hasRigidBody)
        {
            rb.isKinematic = false;
            rb.AddForce(force, ForceMode.Impulse);
        }

        OnThrow?.Invoke(force, fromNetwork);
    }
    public virtual void ExecuteHystericInteraction()
    {
        Throw(
            trans.position,
            new Vector3
                (UnityEngine.Random.Range(0, 1),
                UnityEngine.Random.Range(0.1f, 1),
                UnityEngine.Random.Range(0, 1)
                ) * 10,
            transform.rotation.eulerAngles
            );
    }

    public Vector3 GetColliderSize()
    {
        return mainColliderSize;
    }

    protected virtual void OnDestroy()
    {
        HoldableItem.instances.Remove(gameObject);
        IEnemyHystericObject.instances.Remove(gameObject);
    }
}
