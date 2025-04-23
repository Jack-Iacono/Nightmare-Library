using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(PlayerInteractionController))]
public class InventoryController : MonoBehaviour
{
    public static InventoryController instance;
    private PlayerInteractionController interactionController;
    [SerializeField]
    private Transform handPosition;

    private const int inventorySize = 3;
    private InventoryItem[] inventoryItems = new InventoryItem[inventorySize];

    private int currentItemIndex = 0;

    public static KeyCode keyInventoryUp = KeyCode.Z;
    public static KeyCode keyInventoryDown = KeyCode.X;

    public static InventoryItem currentHeldItem = null;

    private void Awake()
    {
        if(instance != null)
            Destroy(instance.gameObject);
        instance = this;

        interactionController = GetComponent<PlayerInteractionController>();    

        for (int i = 0; i < inventorySize; i++)
        {
            inventoryItems[i] = null;
        }
    }
    private void Start()
    {
        SetCurrentIndex(0);
    }

    private void Update()
    {
        if (!PauseController.gamePaused)
        {
            if (Input.GetKeyDown(keyInventoryUp))
            {
                SetCurrentIndex((currentItemIndex + 1) % inventorySize);
            }
            else if (Input.GetKeyDown(keyInventoryDown))
            {
                SetCurrentIndex((currentItemIndex - 1 + inventorySize) % inventorySize);
            }
        }

        if (currentHeldItem != null && !interactionController.CheckItemPlacing())
        {
            currentHeldItem.holdable.EnableMesh(true);
            currentHeldItem.holdable.trans.position = handPosition.position;
            currentHeldItem.holdable.trans.rotation = handPosition.rotation;
        }
    }

    public void PickupMoveable(HoldableItem moveable)
    {
        bool found = false;
        foreach(InventoryItem i in inventoryItems)
        {
            if (i.holdable == moveable)
                found = true; break;
        }

        if (!found)
        {
            for(int i = 0; i < inventorySize; i++)
            {
                if (inventoryItems[i] == null)
                {
                    inventoryItems[i] = new InventoryItem(moveable);
                    break;
                }
            }
        }
    }

    public InventoryItem GetCurrentItem()
    {
        return currentHeldItem;
    }
    public bool AddItem(HoldableItem item)
    {
        int open = GetOpenInventorySlot();

        if (open != -1)
        {
            inventoryItems[open] = new InventoryItem(item);
            currentItemIndex = open;
        }
        else
            return false;

        currentHeldItem = inventoryItems[currentItemIndex];

        return true;
    }
    public bool RemoveCurrentItem()
    {
        if (inventoryItems[currentItemIndex] != null)
        {
            inventoryItems[currentItemIndex] = null;
            currentHeldItem = null;
            return true;
        }

        return false;
    }

    public InventoryItem[] GetInventoryItems()
    {
        return inventoryItems;
    }
    public HoldableItem[] GetHoldableItems()
    {
        HoldableItem[] items = new HoldableItem[inventoryItems.Length];
        for(int i = 0; i < inventoryItems.Length; i++)
        {
            items[i] = inventoryItems[i].holdable;
        }
        return items;
    }
    public void ClearInventory()
    {
        for(int i = 0; i < inventoryItems.Length; i++)
        {
            inventoryItems[i] = null;
        }

        SetCurrentIndex(0);
    }

    private void SetCurrentIndex(int index)
    {
        currentItemIndex = index;

        // Remove the current item from the player's hands
        if (currentHeldItem != null)
        {
            currentHeldItem.holdable.EnableMesh(false);
            currentHeldItem.holdable.gameObject.SetActive(false);
        }

        currentHeldItem = inventoryItems[currentItemIndex];

        // Show the item in the player's hands if there is one
        if (currentHeldItem != null)
        {
            currentHeldItem.holdable.EnableMesh(true);
            currentHeldItem.holdable.trans.position = handPosition.position;
            currentHeldItem.holdable.trans.rotation = handPosition.rotation;
        }
    }
    
    public bool HasOpenSlot()
    {
        return GetOpenInventorySlot() != -1;
    }
    private int GetOpenInventorySlot()
    {
        for(int i = 0; i < inventoryItems.Length; i++)
        {
            if (inventoryItems[i] == null)
                return i;
        }

        return -1;
    }

    private void OnDestroy()
    {
        if(instance == this)
            instance = null;
    }
}
