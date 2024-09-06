using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    // Used for quick interactable look up
    public static Dictionary<GameObject, Interactable> interactables { get; private set; } = new Dictionary<GameObject, Interactable>();

    public enum PlacementType { FLOOR, WALL, CEILING }
    public List<PlacementType> placementTypes = new List<PlacementType>();
    public bool placePerp = false;

    [SerializeField]
    public bool precisePlacement = false;

    // Contains the mesh renderer as well as the default mesh
    private Dictionary<MeshRenderer, Material> renderMaterialList = new Dictionary<MeshRenderer, Material>();

    private List<Collider> colliders = new List<Collider>();
    private Vector3 mainColliderSize = Vector3.zero;

    private bool hasRigidBody = false;
    protected Rigidbody rb;

    [Header("Interactable Variables")]
    [SerializeField]
    public bool allowPlayerClick = false;
    [SerializeField]
    public bool allowPlayerPickup = false;
    [SerializeField]
    public bool allowEnemyHysterics = false;
    [SerializeField]
    public bool allowEnemyFlicker = false;

    public delegate void OnClickDelegate(bool fromNetwork = false);
    public event OnClickDelegate OnClick;
    public delegate void OnPickupDelegate(bool fromNetwork = false);
    public event OnPickupDelegate OnPickup;
    public delegate void OnPlaceDelegate(bool fromNetwork = false);
    public event OnPlaceDelegate OnPlace;

    public delegate void OnEnemyInteractHystericsDelegate(bool fromNetwork = false);
    public event OnEnemyInteractHystericsDelegate OnEnemyInteractHysterics;
    public delegate void OnEnemyInteractFlickerDelegate(bool fromNetwork = false);
    public event OnEnemyInteractFlickerDelegate OnEnemyInteractFlicker;

    protected virtual void Awake()
    {
        interactables.Add(gameObject, this);

        foreach(MeshRenderer r in GetComponentsInChildren<MeshRenderer>())
        {
            renderMaterialList.Add(r, r.material);
        }

        foreach(Collider col in GetComponentsInChildren<Collider>())
        {
            colliders.Add(col);
        }
        mainColliderSize = colliders[0].bounds.size;

        hasRigidBody = TryGetComponent(out rb);
    }

    public virtual void Click(bool fromNetwork = false)
    {
        OnClick?.Invoke(fromNetwork);
    }
    public virtual void Pickup(bool fromNetwork = false)
    {
        // Decyphers between local pickup and pickup via notification
        if (!fromNetwork)
        {
            if (InventoryController.Instance.AddItem(gameObject))
            {
                EnableColliders(false);
                EnableMesh(false);
                OnPickup?.Invoke(fromNetwork);
            }
        }
        else
        {
            EnableColliders(false);
            EnableMesh(false);
            OnPickup?.Invoke(fromNetwork);
        }
        
    }

    public virtual void Place(bool fromNetwork = false)
    {
        EnableColliders(true);
        EnableMesh(true);
        ResetMeshMaterial();

        OnPlace?.Invoke(fromNetwork);
    }
    public virtual void Place(Vector3 pos, Quaternion rot, bool fromNetwork = false)
    {
        transform.position = pos;
        transform.rotation = rot;

        Place(fromNetwork);
    }

    public virtual void EnemyInteractHysterics(bool fromNetwork = false)
    {
        OnEnemyInteractHysterics?.Invoke(fromNetwork);
    }
    public virtual void EnemyInteractFlicker(bool fromNetwork = false)
    {
        OnEnemyInteractFlicker?.Invoke(fromNetwork);
    }

    public void EnableColliders(bool b)
    {
        foreach(Collider c in colliders)
        {
            c.enabled = b;
        }
        if (hasRigidBody)
            rb.isKinematic = !b;
    }

    public void EnableMesh(bool b)
    {
        foreach(MeshRenderer r in renderMaterialList.Keys)
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
        interactables.Remove(gameObject);
    }
}
