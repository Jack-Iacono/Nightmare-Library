using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    // Used for quick interactable look up
    public static Dictionary<GameObject, Interactable> interactables { get; private set; } = new Dictionary<GameObject, Interactable>();
    public Transform trans { get; protected set; }

    [SerializeField]
    private GameObject gameobjectOverride = null;

    public enum PlacementType { FLOOR, WALL, CEILING }
    public List<PlacementType> placementTypes = new List<PlacementType>();

    public bool isPhysical { get; private set; } = true;

    /// <summary>
    /// 0: Facing Player 
    /// 1: Facing away from player
    /// </summary>
    [Range(0,1)]
    public int floorPlacementType = 0;
    /// <summary>
    /// 0: Bottom down, no x or z rotation, facing player
    /// 1: Bottom on wall, top facing player
    /// </summary>
    [Range(0,1)]
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

    [Header("Interactable Variables")]
    [SerializeField]
    public bool allowPlayerClick = false;
    [SerializeField]
    public bool allowPlayerPickup = false;
    [SerializeField]
    public bool allowEnemyHysterics = false;
    [SerializeField]
    public bool allowEnemyFlicker = false;

    public delegate void OnClickDelegate(Interactable interactable, bool fromNetwork = false);
    public event OnClickDelegate OnClick;
    public delegate void OnPickupDelegate(bool fromNetwork = false);
    public event OnPickupDelegate OnPickup;
    public delegate void OnPlaceDelegate(bool fromNetwork = false);
    public event OnPlaceDelegate OnPlace;
    public delegate void OnThrowDelegate(Vector3 force, bool fromNetwork = false);
    public event OnThrowDelegate OnThrow;

    public delegate void OnAllEnabledDelegate(bool enabled);
    public event OnAllEnabledDelegate OnSetPhysical;

    public delegate void OnEnemyInteractHystericsDelegate(bool fromNetwork = false);
    public event OnEnemyInteractHystericsDelegate OnEnemyInteractHysterics;
    public delegate void OnEnemyInteractFlickerDelegate(bool fromNetwork = false);
    public event OnEnemyInteractFlickerDelegate OnEnemyInteractFlicker;

    [Header("Flicker Variables")]
    public Light attachedLight;

    int LightFlickerAvg = 5;
    int lightFlickerDev = 2;

    float lightFlickerDurationAvg = 0.2f;
    float lightFlickerDurationDev = 0.19f;

    float lightFlickerCooldownAvg = 1f;
    float lightFlickerCooldownDev = 0.9f;

    private bool isFlickering = false;

    protected virtual void Awake()
    {
        // Add this object to the dictionary for easy referncing from other scripts via the gameObject
        if (gameobjectOverride == null)
            interactables.Add(gameObject, this);
        else
            interactables.Add(gameobjectOverride, this);

        // Find the renderers present on this object for use with placement
        foreach (MeshRenderer r in GetComponentsInChildren<MeshRenderer>())
        {
            renderMaterialList.Add(r, r.material);
        }

        // Find all the colliders associated with this object
        foreach(Collider col in GetComponentsInChildren<Collider>())
        {
            colliders.Add(col);
        }

        if (colliders.Count > 0)
            mainColliderSize = colliders[0].bounds.size;
        else
            mainColliderSize = Vector3.zero;

        hasRigidBody = TryGetComponent(out rb);
        trans = transform;
    }

    /// <summary>
    /// Called by the player interaction controller, notifies this interactable about a change in the hover state
    /// </summary>
    /// <param name="onOff">True: The interactable is being hovered over || False: The interactable has stopped being hovered over</param>
    public virtual void Hover(bool onOff) { }

    public virtual void Click(bool fromNetwork = false)
    {
        OnClick?.Invoke(this, fromNetwork);
    }
    public virtual GameObject Pickup(bool fromNetwork = false)
    {
        SetPhysical(false);

        if (hasRigidBody)
            rb.isKinematic = true;

        OnPickup?.Invoke(fromNetwork);

        return gameObject;
    }

    public virtual void Place(bool fromNetwork = false)
    {
        SetPhysical(true);

        if (fixPlacement && hasRigidBody)
            rb.isKinematic = true;

        OnPlace?.Invoke(fromNetwork);
    }
    public virtual void Place(Vector3 pos, Quaternion rot, bool fromNetwork = false)
    {
        trans.position = pos;
        trans.rotation = rot;

        Place(fromNetwork);
    }

    public virtual void Throw(Vector3 pos, Vector3 force, bool fromNetwork = false)
    {
        trans.position = pos;
        SetPhysical(true);

        if (hasRigidBody)
        {
            rb.isKinematic = false;
            rb.AddForce(force, ForceMode.Impulse);
        }

        OnThrow?.Invoke(force, fromNetwork);
    }

    public virtual void EnemyInteractHysterics(bool fromNetwork = false)
    {
        rb.isKinematic = false;
        rb.AddForce
            (
            new Vector3
                (UnityEngine.Random.Range(0, 1),
                UnityEngine.Random.Range(0.1f, 1),
                UnityEngine.Random.Range(0, 1)
                ) * 10,
            ForceMode.Impulse
            );

        OnEnemyInteractHysterics?.Invoke(fromNetwork);
    }
    public virtual void EnemyInteractFlicker(bool fromNetwork = false)
    {
        if (!isFlickering)
            StartCoroutine(FlickerLightCoroutine());
        OnEnemyInteractFlicker?.Invoke(fromNetwork);
    }
    IEnumerator FlickerLightCoroutine()
    {
        isFlickering = true;

        int flickerAmount = UnityEngine.Random.Range(LightFlickerAvg - lightFlickerDev, LightFlickerAvg + lightFlickerDev);
        for (int i = 0; i < flickerAmount; i++)
        {
            attachedLight.enabled = false;
            yield return new WaitForSeconds(UnityEngine.Random.Range(lightFlickerDurationAvg - lightFlickerDurationDev, lightFlickerDurationAvg + lightFlickerDurationDev));
            attachedLight.enabled = true;
            yield return new WaitForSeconds(UnityEngine.Random.Range(lightFlickerCooldownAvg - lightFlickerCooldownDev, lightFlickerCooldownAvg + lightFlickerCooldownDev));
        }

        isFlickering = false;
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
        foreach(Collider c in colliders)
        {
            c.enabled = b;
        }
        if (!b && hasRigidBody)
            rb.isKinematic = true;

        isPhysical = b;
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
