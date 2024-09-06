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

    [SerializeField]
    public bool precisePlacement = false;

    // Contains the mesh renderer as well as the default mesh
    private Dictionary<MeshRenderer, Material> renderMaterialList = new Dictionary<MeshRenderer, Material>();
    private List<Collider> colliders = new List<Collider>();

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
    }

    public virtual void Click(bool fromNetwork = false)
    {
        OnClick?.Invoke(fromNetwork);
    }
    public virtual void Pickup(bool fromNetwork = false)
    {
        if (InventoryController.Instance.AddItem(gameObject))
        {
            EnableColliders(false);
            EnableMesh(false);
            OnPickup?.Invoke(fromNetwork);
        }
    }

    public virtual void Place(RaycastHit hit, bool fromNetwork = false)
    {
        transform.position = hit.point;
        transform.LookAt(hit.point + hit.normal);
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        Place(transform.position, transform.rotation, fromNetwork);
    }
    public virtual void Place(Vector3 pos, Quaternion rot, bool fromNetwork = false)
    {
        gameObject.SetActive(true);

        transform.position = pos;
        transform.rotation = rot;

        EnableColliders(true);
        EnableMesh(true);
        ResetMeshMaterial();

        OnPlace?.Invoke(fromNetwork);
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

    private void OnDestroy()
    {
        interactables.Remove(gameObject);
    }
}
