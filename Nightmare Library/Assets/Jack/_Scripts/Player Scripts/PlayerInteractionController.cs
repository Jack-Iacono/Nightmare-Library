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
    private InventoryController inventoryCont;

    [SerializeField]
    private GameObject placementGuideprefab;
    private PlacementGuideController placementGuideController;

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

    private bool isPlacingItem = false;
    private bool isPlacementValid = false;
    private bool isPlacementVisible = false;

    public bool canSeeItem { get; private set; }
    public delegate void OnItemSightChangeDelegate(int interactionType);
    public static event OnItemSightChangeDelegate onItemSightChange;

    // Start is called before the first frame update
    void Start()
    {
        playerCont = GetComponent<PlayerController>();
        inventoryCont = playerCont.inventoryCont;

        GameObject guide = Instantiate(placementGuideprefab, playerCont.transform);
        placementGuideController = guide.GetComponent<PlacementGuideController>();
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

        if (currentInventoryItem != null)
        {
            currentHoldableItem = currentInventoryItem.holdable;
            currentUsableItem = currentInventoryItem.useable;

            placementGuideController.SetMeshFilter(currentHoldableItem.mainMeshFilter);
        }

        // Process Throwing first since you don't need to raycast for it
        if (currentHoldableItem != null && isThrow)
        {
            // TEMPORARY
            // Not sure where to throw from or what velocity to have
            currentHoldableItem.Throw(transform.position + transform.forward + transform.up, ray.direction * 10, transform.rotation.eulerAngles);
            inventoryCont.RemoveCurrentItem();
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
            else
            {
                onItemSightChange?.Invoke(-1);
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
                else if (isPickup && interactionTypes[1] == true &&inventoryCont.HasOpenSlot())
                {
                    inventoryCont.AddItem(HoldableItem.instances[hitObject]);
                    HoldableItem.instances[hitObject].Pickup();

                    actionBuffering = true;
                }
                else if (currentHoldableItem != null)
                {
                    // For starting or resuming the placement
                    if (isPlaceStart || (isPlacePressed && !isPlacingItem))
                    {
                        placementGuideController.SetMaterial(true);
                        placementGuideController.SetVisibility(true);

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
                                        placementGuideController.trans.Rotate(Vector3.up, Input.GetAxis("Mouse X") * 100 * Time.deltaTime);
                                        break;
                                    case PlacementType.WALL:
                                        if (currentHoldableItem.wallPlacementType == 0)
                                            placementGuideController.trans.Rotate(Vector3.forward, Input.GetAxis("Mouse X") * 100 * Time.deltaTime);
                                        else
                                            placementGuideController.trans.Rotate(Vector3.up, Input.GetAxis("Mouse X") * 100 * Time.deltaTime);
                                        break;
                                }
                            }
                            else
                            {
                                SetObjectTransform(type, hit);
                            }

                            // Check if the object is colliding with other stuff
                            if (!Physics.CheckBox(placementGuideController.trans.position, currentHoldableItem.GetColliderSize() * 0.45f, placementGuideController.trans.rotation, interactLayers))
                            {
                                placementGuideController.SetMaterial(true);
                                isPlacementValid = true;
                            }
                            else
                            {
                                placementGuideController.SetMaterial(false);
                                isPlacementValid = false;
                            }

                            isPlacementVisible = true;
                        }
                        else
                        {
                            isPlacementVisible = false;
                        }
                    }
                    else if (isPlaceFinish && isPlacingItem)
                    {
                        if (isPlacementValid)
                        {
                            PlacementType type = CheckPlacementType(hit);

                            if (currentHoldableItem.placementTypes.Contains(type))
                            {
                                currentHoldableItem.Place(placementGuideController.trans.position, placementGuideController.trans.rotation);
                               inventoryCont.RemoveCurrentItem();
                            }

                            currentHoldableItem = null;
                            actionBuffering = true;
                        }
                        
                        placementGuideController.SetVisibility(false);

                        isPlacingItem = false;
                        playerCont.Lock(false);
                    }
                }
            }
        }
        else if (isPlacingItem)
        {
            // Resets the item if it was being placed and the player is now too far from the placement range
            placementGuideController.SetVisibility(false);
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
        if(inventoryCont != null)
        {
            HoldableItem[] items = inventoryCont.GetHoldableItems();
            for (int i = 0; i < items.Length; i++)
            {
                HoldableItem item = items[i];

                if (item != null)
                {
                    //NavMeshHit hit;
                    //NavMesh.SamplePosition(transform.position + new Vector3(0,i,0), out hit, 10, NavMesh.AllAreas);
                    item.Throw(transform.position + new Vector3(0, i, 0), Vector3.up * 5, transform.rotation.eulerAngles);
                }
            }
            inventoryCont.ClearInventory();
        }
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

                placementGuideController.trans.position = hit.point + MultiplyVector(hit.normal, currentHeldItem.GetColliderSize()) / 2;
                placementGuideController.trans.rotation = Quaternion.Euler(xRot, yRot, zRot);
                break;
            case PlacementType.WALL:
                float wallAngleY = Mathf.Atan2(hit.normal.x, hit.normal.z) * Mathf.Rad2Deg;
                
                switch (currentHeldItem.wallPlacementType)
                {
                    case 0:
                        // Back against wall, facing perpendicular
                        placementGuideController.trans.position = hit.point + hit.normal * currentHeldItem.GetColliderSize().z / 2;
                        placementGuideController.trans.rotation = Quaternion.Euler(0, wallAngleY, 0);
                        break;
                    case 1:
                        // Have bottom against the wall
                        placementGuideController.trans.position = hit.point + hit.normal * currentHeldItem.GetColliderSize().y / 2;
                        placementGuideController.trans.rotation = Quaternion.Euler(90, wallAngleY, 0);
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

    public bool CheckItemPlacing()
    {
        return isPlacementVisible;
    }

}
