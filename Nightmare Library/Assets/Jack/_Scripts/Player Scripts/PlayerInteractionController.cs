using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static Interactable;

[RequireComponent(typeof(PlayerController))]
public class PlayerInteractionController : MonoBehaviour
{
    private PlayerController playerCont;

    [SerializeField]
    private float interactDistance = 4f;
    [SerializeField]
    private LayerMask interactLayers;

    private static KeyCode keyPickup = KeyCode.E;
    private static KeyCode keyPlace = KeyCode.F;
    private static KeyCode keyThrow = KeyCode.T;
    private static KeyCode keyClick = KeyCode.Mouse0;

    private bool isActive = false;
    private bool isPickup = false;
    private bool isClick = false;
    private bool isThrow = false;

    private bool isPlaceStart = false;
    private bool isPlacePressed = false;
    private bool isPlaceFinish = false;

    private float actionBufferTime = 0.25f;
    private float actionBufferTimer = 0f;
    private bool actionBuffering = false;

    [SerializeField]
    private Material clearPlacementMaterial;
    [SerializeField]
    private Material blockedPlacementMaterial;
    private bool isPlacingItem = false;
    private Interactable currentHeldItem;
    private bool isPlacementValid = false;

    public bool canSeeItem { get; private set; }
    public delegate void OnItemSightChangeDelegate(int interactionType);
    public static event OnItemSightChangeDelegate onItemSightChange;

    private Interactable currentSeenItem;
    private int currentSightType = -1;

    // Start is called before the first frame update
    void Start()
    {
        playerCont = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameController.gamePaused)
        {
            CheckActionBuffer();
            GetInput();
            Check();
        }
    }

    private void CheckActionBuffer()
    {
        if(actionBuffering)
        {
            actionBufferTimer -= Time.deltaTime;

            if(actionBufferTimer <= 0f)
            {
                actionBufferTimer = actionBufferTime;
                actionBuffering = false;
            }
        }
    }
    private void GetInput()
    {
        isClick = Input.GetKeyDown(keyClick);
        isPickup = Input.GetKeyDown(keyPickup);
        isThrow = Input.GetKeyDown(keyThrow);

        isPlaceStart = Input.GetKeyDown(keyPlace);
        isPlaceFinish = Input.GetKeyUp(keyPlace);
        isPlacePressed = Input.GetKey(keyPlace);
        
        isActive = isClick || isPickup || isPlaceFinish || isPlacePressed || isPlaceStart || isThrow;
    }
    private void Check()
    {
        // Create the ray that represents where the player is looking
        Ray ray = playerCont.camCont.GetCameraRay();
        RaycastHit hit;
        bool raycastHit = false;

        // Used for changing reticle to indicate when player is looking at an interactable
        if(Physics.Raycast(ray, out hit, interactDistance, interactLayers))
        {
            raycastHit = true;
            if (interactables.ContainsKey(hit.collider.gameObject))
            {
                // Assign the default type to "null"
                int type = -1;
                // Store the interaactable that is currently being viewed
                Interactable itemInView = interactables[hit.collider.gameObject];

                // Check to see which interaction can happen for the viewed item
                if (itemInView.allowPlayerClick)
                    type = 0;
                else if (itemInView.allowPlayerPickup)
                    type = 1;

                // Send message to items that are being hovered or are being un-hovered
                if(itemInView != currentSeenItem)
                {
                    if (currentSeenItem != null)
                        currentSeenItem.Hover(false);
                    itemInView.Hover(true);
                    currentSeenItem = itemInView;
                }
                    
                if (!canSeeItem || currentSightType != type)
                {
                    onItemSightChange?.Invoke(type);
                    currentSightType = type;
                }

                canSeeItem = true;
            }
            else
            {
                if (canSeeItem)
                    onItemSightChange?.Invoke(-1);
                canSeeItem = false;

                if (currentSeenItem != null)
                {
                    currentSeenItem.Hover(false);
                    currentSeenItem = null;
                }
            }
        }
        else
        {
            if (canSeeItem)
                onItemSightChange?.Invoke(-1);
            canSeeItem = false;

            if(currentSeenItem != null)
            {
                currentSeenItem.Hover(false);
                currentSeenItem = null;
            }
        }

        if (!actionBuffering && isActive)
        {
            // Get the currently held item, if there is one, from the Inventory controller
            InventoryItem temp = InventoryController.instance.GetCurrentItem();
            if (!temp.IsEmpty())
                currentHeldItem = interactables[temp.realObject];

            // Check if the player is throwing the current item
            if (currentHeldItem != null && isThrow)
            {
                // TEMPORARY
                // Not sure where to throw from or what velocity to have
                currentHeldItem.Throw(transform.position + transform.forward + transform.up, ray.direction * 10);
                InventoryController.instance.RemoveCurrentItem();
                currentHeldItem = null;
                actionBuffering = true;
            }

            // Check if the player is looking at a surface
            if (raycastHit)
            {
                // Check if the player is holding an item
                if (currentHeldItem != null)
                {
                    if (isPlaceStart || (isPlacePressed && !isPlacingItem))
                    {
                        // Working like this as it will not be networked this way
                        currentHeldItem.gameObject.SetActive(true);
                        currentHeldItem.EnableColliders(false);
                        currentHeldItem.SetMeshMaterial(clearPlacementMaterial);
                        currentHeldItem.EnableMesh(true);

                        if (currentHeldItem.precisePlacement)
                        {
                            PlacementType type = CheckPlacementType(hit);

                            SetObjectTransform(type, hit);

                            playerCont.Lock(true);
                        }

                        isPlacingItem = true;
                    }
                    else if (isPlacePressed && isPlacingItem)
                    {
                        PlacementType type = CheckPlacementType(hit);

                        if (currentHeldItem != null && currentHeldItem.placementTypes.Contains(type))
                        {
                            if (currentHeldItem.precisePlacement)
                            {
                                switch (type)
                                {
                                    case PlacementType.FLOOR:
                                        currentHeldItem.transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * 100 * Time.deltaTime);
                                        break;
                                    case PlacementType.WALL:
                                        if(currentHeldItem.wallPlacementType == 0)
                                            currentHeldItem.transform.Rotate(Vector3.forward, Input.GetAxis("Mouse X") * 100 * Time.deltaTime);
                                        else
                                            currentHeldItem.transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * 100 * Time.deltaTime);
                                        break;
                                }
                            }
                            else
                            {
                                SetObjectTransform(type, hit);
                            }
                        }

                        // Check if the object is colliding with other stuff
                        if (!Physics.CheckBox(currentHeldItem.transform.position, currentHeldItem.GetColliderSize() * 0.45f, currentHeldItem.transform.rotation, interactLayers))
                        {
                            currentHeldItem.SetMeshMaterial(clearPlacementMaterial);
                            isPlacementValid = true;
                        }
                        else
                        {
                            currentHeldItem.SetMeshMaterial(blockedPlacementMaterial);
                            isPlacementValid = false;
                        }
                    }
                    else if (isPlaceFinish && isPlacingItem)
                    {
                        if (isPlacementValid)
                        {
                            PlacementType type = CheckPlacementType(hit);

                            if (currentHeldItem.placementTypes.Contains(type))
                            {
                                currentHeldItem.Place();
                                InventoryController.instance.RemoveCurrentItem();
                            }

                            currentHeldItem = null;
                            actionBuffering = true;
                        }
                        else
                        {
                            // Working like this as it will not be networked this way
                            currentHeldItem.gameObject.SetActive(false);
                            currentHeldItem.EnableMesh(false);
                        }

                        isPlacingItem = false;
                        playerCont.Lock(false);
                    }
                }

                // Check for interactions that aren't placing or throwing
                if (canSeeItem)
                {
                    if(isClick && interactables[hit.collider.gameObject].allowPlayerClick)
                        interactables[hit.collider.gameObject].Click();
                    else if (isPickup && interactables[hit.collider.gameObject].allowPlayerPickup && InventoryController.instance.HasOpenSlot())
                    {
                        InventoryController.instance.AddItem(interactables[hit.collider.gameObject].Pickup());
                    }

                    actionBuffering = true;
                }
            }
            else if(isPlacingItem)
            {
                // Resets the item if it was being placed and the player is now too far from the placement range
                currentHeldItem.ResetMeshMaterial();
                currentHeldItem.EnableMesh(false);
                isPlacingItem = false;
                playerCont.Lock(false);
            }
        }
    }

    public void DropItems()
    {
        InventoryItem[] items = InventoryController.instance.GetInventoryItems();
        for(int i = 0; i < items.Length; i++)
        {
            InventoryItem item = items[i];

            if (!item.IsEmpty())
            {
                //NavMeshHit hit;
                //NavMesh.SamplePosition(transform.position + new Vector3(0,i,0), out hit, 10, NavMesh.AllAreas);
                interactables[item.realObject].Place(transform.position + new Vector3(0, i, 0), transform.rotation);
            }
        }
        InventoryController.instance.ClearInventory();
    }

    private void SetObjectTransform(PlacementType type, RaycastHit hit)
    {
        switch (type)
        {
            case PlacementType.FLOOR:
                // Find the slope of the wall that is being hit. This would be the x rotation if the player is facing 0 degrees
                float slope = Mathf.RoundToInt(Mathf.Atan2(Mathf.Sqrt(-hit.normal.x * -hit.normal.x + hit.normal.z * hit.normal.z), hit.normal.y) * Mathf.Rad2Deg);
                // Find the y angle of the normal vector of the raycasthit
                float hitAngle = Mathf.RoundToInt(Mathf.Atan2(hit.normal.x, hit.normal.z) * Mathf.Rad2Deg);

                // Get the various components of the rotation
                float yRot = 0;
                switch (currentHeldItem.floorPlacementType)
                {
                    case 0:
                        // Face toward the player
                        yRot = transform.rotation.eulerAngles.y - 180;
                        break;
                    case 1:
                        // Face away from the player
                        yRot = transform.rotation.eulerAngles.y;
                        break;
                }
                float xRot = Mathf.Cos((yRot - hitAngle) * Mathf.Deg2Rad) * slope;
                float zRot = Mathf.Sin((yRot - hitAngle) * Mathf.Deg2Rad) * slope;

                currentHeldItem.transform.position = hit.point + MultiplyVector(hit.normal, currentHeldItem.GetColliderSize()) / 2;
                currentHeldItem.transform.rotation = Quaternion.Euler(xRot, yRot, zRot);
                break;
            case PlacementType.WALL:
                float wallAngleY = Mathf.Atan2(hit.normal.x, hit.normal.z) * Mathf.Rad2Deg;
                
                switch (currentHeldItem.wallPlacementType)
                {
                    case 0:
                        // Back against wall, facing perpendicular
                        currentHeldItem.transform.position = hit.point + hit.normal * currentHeldItem.GetColliderSize().z / 2;
                        currentHeldItem.transform.rotation = Quaternion.Euler(0, wallAngleY, 0);
                        break;
                    case 1:
                        // Have bottom against the wall
                        currentHeldItem.transform.position = hit.point + hit.normal * currentHeldItem.GetColliderSize().y / 2;
                        currentHeldItem.transform.rotation = Quaternion.Euler(90, wallAngleY, 0);
                        break;
                }
                break;
        }
    }
    private Vector3 MultiplyVector(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    private PlacementType CheckPlacementType(RaycastHit hit)
    {
        // dictate what kind of surface the object is being placed on
        switch (Mathf.RoundToInt(hit.normal.y))
        {
            case > 0:
                return PlacementType.FLOOR;
            case 0:
                return PlacementType.WALL;
            case < 0:
                return PlacementType.CEILING;
        }
    }

}
