using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using static Interactable;
using static UnityEditor.Progress;

[RequireComponent(typeof(PlayerController))]
public class PlayerInteractionController : MonoBehaviour
{
    private PlayerController playerCont;

    [SerializeField]
    private float interactDistance = 10f;
    [SerializeField]
    private LayerMask interactLayers;

    private static KeyCode keyPickup = KeyCode.E;
    private static KeyCode keyPlace = KeyCode.F;
    private static KeyCode keyClick = KeyCode.Mouse0;

    private bool isActive = false;
    private bool isPickup = false;
    private bool isClick = false;

    private bool isPlaceStart = false;
    private bool isPlacing = false;
    private bool isPlaceFinish = false;

    private float actionBufferTime = 0.5f;
    private float actionBufferTimer = 0f;
    private bool actionBuffering = false;

    [SerializeField]
    private Material placementMaterial;
    private bool isPlacingItem = false;
    private Interactable currentPlacingItem;

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

        isPlaceStart = Input.GetKeyDown(keyPlace);
        isPlaceFinish = Input.GetKeyUp(keyPlace);
        isPlacing = Input.GetKey(keyPlace);
        
        isActive = isClick || isPickup || isPlaceFinish || isPlacing || isPlaceStart;
    }
    private void Check()
    {
        if (!actionBuffering && isActive)
        {
            Ray ray = playerCont.camCont.GetCameraRay();
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactDistance, interactLayers))
            {
                InventoryItem temp = InventoryController.Instance.GetCurrentItem();
                if (!temp.IsEmpty())
                    currentPlacingItem = interactables[temp.realObject];

                if (currentPlacingItem != null)
                {
                    if (isPlaceStart)
                    {
                        currentPlacingItem.SetMeshMaterial(placementMaterial);
                        currentPlacingItem.EnableMesh(true);

                        if (currentPlacingItem.precisePlacement)
                        {
                            currentPlacingItem.transform.position = hit.point;
                            playerCont.Lock(true);
                        }

                        isPlacingItem = true;
                    }
                    else if (isPlacing && isPlacingItem)
                    {
                        PlacementType type = CheckPlacementType(hit);

                        if (currentPlacingItem != null && currentPlacingItem.placementTypes.Contains(type))
                        {
                            if (currentPlacingItem.precisePlacement)
                            {
                                Debug.Log(Input.GetAxis("Mouse X"));
                                currentPlacingItem.transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * 100 * Time.deltaTime);
                            }
                            else
                            {
                                currentPlacingItem.transform.position = hit.point;
                                currentPlacingItem.transform.LookAt(hit.point + hit.normal);
                            }
                        }
                    }
                    else if (isPlaceFinish && isPlacingItem)
                    {
                        PlacementType type = CheckPlacementType(hit);

                        if (currentPlacingItem.placementTypes.Contains(type))
                        {
                            switch (type)
                            {
                                case PlacementType.FLOOR:
                                    currentPlacingItem.Place(hit.point, currentPlacingItem.transform.rotation);
                                    break;
                                case PlacementType.CEILING:
                                    currentPlacingItem.Place(hit);
                                    break;
                                case PlacementType.WALL:
                                    currentPlacingItem.Place(hit);
                                    break;
                            }

                            InventoryController.Instance.RemoveCurrentItem();
                        }

                        isPlacingItem = false;
                        playerCont.Lock(false);
                        currentPlacingItem = null;
                        actionBuffering = true;
                    }
                }

                if (interactables.ContainsKey(hit.collider.gameObject))
                {
                    if(isClick)
                        interactables[hit.collider.gameObject].Click();
                    else if (isPickup && InventoryController.Instance.HasOpenSlot())
                        interactables[hit.collider.gameObject].Pickup();

                    actionBuffering = true;
                }
            }
            else if(isPlacingItem)
            {
                // Resets the item if it was being placed and the player is now too far from the placement range
                currentPlacingItem.ResetMeshMaterial();
                currentPlacingItem.EnableMesh(false);
                isPlacingItem = false;
                playerCont.Lock(false);
            }
        }
    }

    private void GetPlacementOffset(RaycastHit hit)
    {
        // This will rely on the object having an origin point on it's bottom center
        //float wallAngleY = Mathf.Atan2(hit.normal.x, hit.normal.z) * Mathf.Rad2Deg;
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
