using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static HoldableItem;

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

    private float actionBufferTime = 0.1f;
    private float actionBufferTimer = 0f;
    private bool actionBuffering = false;

    [SerializeField]
    private Material clearPlacementMaterial;
    [SerializeField]
    private Material blockedPlacementMaterial;
    private bool isPlacingItem = false;
    private bool isPlacementValid = false;

    public bool canSeeItem { get; private set; }
    public delegate void OnItemSightChangeDelegate(int interactionType);
    public static event OnItemSightChangeDelegate onItemSightChange;

    // Start is called before the first frame update
    void Start()
    {
        playerCont = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!PauseController.gamePaused)
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

        // Get interactable properties of the held item
        InventoryItem currentInventoryItem = InventoryController.currentHeldItem;
        HoldableItem currentHoldableItem = null;
        IUseable currentUsableItem = null;

        if (!currentInventoryItem.IsEmpty())
        {
            currentHoldableItem = currentInventoryItem.holdable;
            currentUsableItem = currentInventoryItem.useable;
        }

        // Process Throwing first since you don't need to raycast for it
        if (currentHoldableItem != null && isThrow)
        {
            // TEMPORARY
            // Not sure where to throw from or what velocity to have
            currentHoldableItem.Throw(transform.position + transform.forward + transform.up, ray.direction * 10);
            InventoryController.instance.RemoveCurrentItem();
            currentHoldableItem = null;
            actionBuffering = true;
        }
        // Using the click button temporarily to use item
        else if (currentUsableItem != null && isClick)
        {
            currentUsableItem.Use();
            actionBuffering = true;
        }

        // Used for changing reticle to indicate when player is looking at an interactable
        if (Physics.Raycast(ray, out hit, interactDistance, interactLayers))
        {
            // Get the gameobject that was hit
            GameObject hitObject = hit.collider.gameObject;

            // Used to represent whether the item can be clicked, interacted with, etc
            // 0: Click, 1: Pickup
            bool[] interactionTypes = new bool[2] { false, false };

            // Determine what kind of object the ray is hitting, and figure out if it is clickable or holdable
            if (IClickable.instances.ContainsKey(hitObject))
            {
                interactionTypes[0] = true;
                onItemSightChange?.Invoke(0);
            }   
            else if (HoldableItem.instances.ContainsKey(hitObject))
            { 
                interactionTypes[1] = true;
                onItemSightChange?.Invoke(1);
            }

            // If there is no action buffering and there is some input being processed
            if(!actionBuffering && isActive)
            {
                // Check
                if (isClick && interactionTypes[0] == true)
                {
                    IClickable.instances[hitObject].Click();

                    actionBuffering = true;
                }
                else if (isPickup && interactionTypes[1] == true && InventoryController.instance.HasOpenSlot())
                {
                    InventoryController.instance.AddItem(HoldableItem.instances[hitObject]);
                    HoldableItem.instances[hitObject].Pickup();

                    actionBuffering = true;
                }
                else if (currentHoldableItem != null)
                {
                    // For starting or resuming the placement
                    if (isPlaceStart || (isPlacePressed && !isPlacingItem))
                    {
                        // Working like this as it will not be networked this way
                        currentHoldableItem.gameObject.SetActive(true);
                        currentHoldableItem.EnableColliders(false);
                        currentHoldableItem.SetMeshMaterial(clearPlacementMaterial);
                        currentHoldableItem.EnableMesh(true);

                        if (currentHoldableItem.precisePlacement)
                        {
                            PlacementType type = CheckPlacementType(hit);
                            SetObjectTransform(type, hit);
                            playerCont.Lock(true);
                        }

                        isPlacingItem = true;
                    }
                    // Handles moving the item while it is already being placed
                    else if (isPlacePressed && isPlacingItem)
                    {
                        PlacementType type = CheckPlacementType(hit);

                        if (currentHoldableItem != null && currentHoldableItem.placementTypes.Contains(type))
                        {
                            if (currentHoldableItem.precisePlacement)
                            {
                                switch (type)
                                {
                                    case PlacementType.FLOOR:
                                        currentHoldableItem.transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * 100 * Time.deltaTime);
                                        break;
                                    case PlacementType.WALL:
                                        if (currentHoldableItem.wallPlacementType == 0)
                                            currentHoldableItem.transform.Rotate(Vector3.forward, Input.GetAxis("Mouse X") * 100 * Time.deltaTime);
                                        else
                                            currentHoldableItem.transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * 100 * Time.deltaTime);
                                        break;
                                }
                            }
                            else
                            {
                                SetObjectTransform(type, hit);
                            }
                        }

                        // Check if the object is colliding with other stuff
                        if (!Physics.CheckBox(currentHoldableItem.transform.position, currentHoldableItem.GetColliderSize() * 0.45f, currentHoldableItem.transform.rotation, interactLayers))
                        {
                            currentHoldableItem.SetMeshMaterial(clearPlacementMaterial);
                            isPlacementValid = true;
                        }
                        else
                        {
                            currentHoldableItem.SetMeshMaterial(blockedPlacementMaterial);
                            isPlacementValid = false;
                        }
                    }
                    else if (isPlaceFinish && isPlacingItem)
                    {
                        if (isPlacementValid)
                        {
                            PlacementType type = CheckPlacementType(hit);

                            if (currentHoldableItem.placementTypes.Contains(type))
                            {
                                currentHoldableItem.Place(currentHoldableItem.trans.position, currentHoldableItem.trans.rotation);
                                InventoryController.instance.RemoveCurrentItem();
                            }

                            currentHoldableItem = null;
                            actionBuffering = true;
                        }
                        else
                        {
                            // Working like this as it will not be networked this way
                            currentHoldableItem.gameObject.SetActive(false);
                            currentHoldableItem.EnableMesh(false);
                        }

                        isPlacingItem = false;
                        playerCont.Lock(false);
                    }
                }
            }
        }
        else if (isPlacingItem)
        {
            // Resets the item if it was being placed and the player is now too far from the placement range
            currentHoldableItem.ResetMeshMaterial();
            currentHoldableItem.EnableMesh(false);
            isPlacingItem = false;
            playerCont.Lock(false);
        }
        else
        {
            onItemSightChange?.Invoke(-1);
        }
    }

    public void DropItems()
    {
        HoldableItem[] items = InventoryController.instance.GetHoldableItems();
        for(int i = 0; i < items.Length; i++)
        {
            HoldableItem item = items[i];

            if (item != null)
            {
                //NavMeshHit hit;
                //NavMesh.SamplePosition(transform.position + new Vector3(0,i,0), out hit, 10, NavMesh.AllAreas);
                item.Place(transform.position + new Vector3(0, i, 0), transform.rotation);
            }
        }
        InventoryController.instance.ClearInventory();
    }

    private void SetObjectTransform(PlacementType type, RaycastHit hit)
    {
        HoldableItem currentHeldItem = InventoryController.currentHeldItem.holdable;

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
