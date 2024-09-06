using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using static Interactable;

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
    public Material placementMaterial;

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
                if (isPlaceStart)
                {
                    Interactable item = interactables[InventoryController.Instance.GetCurrentItem().realObject];
                    item.SetMeshMaterial(placementMaterial);
                    item.EnableMesh(true);
                }
                else if (isPlacing)
                {
                    GameObject item = InventoryController.Instance.GetCurrentItem().realObject;
                    item.transform.position = hit.point;
                    item.transform.Rotate(Vector3.up, 10f * Time.deltaTime);
                }
                else if (isPlaceFinish)
                {
                    Interactable item = interactables[InventoryController.Instance.GetCurrentItem().realObject];
                    PlacementType type = CheckPlacementType(hit);
                    
                    if (item != null && item.placementTypes.Contains(type))
                    {
                        switch(type)
                        {
                            case PlacementType.FLOOR:
                                item.Place(hit.point, item.transform.rotation);
                                break;
                            case PlacementType.CEILING:
                                item.Place(hit);
                                break;
                            case PlacementType.WALL:
                                item.Place(hit);
                                break;
                        }

                        InventoryController.Instance.RemoveCurrentItem();
                    }

                    actionBuffering = true;
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
