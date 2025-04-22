using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public static InventoryController instance;

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

        for (int i = 0; i < inventorySize; i++)
        {
            inventoryItems[i] = new InventoryItem();
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
                    inventoryItems[i].Set(moveable);
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
            inventoryItems[open].Set(item);
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
        currentHeldItem = inventoryItems[currentItemIndex];
    }
    
    public bool HasOpenSlot()
    {
        return GetOpenInventorySlot() != -1;
    }
    private int GetOpenInventorySlot()
    {
        for(int i = 0; i < inventoryItems.Length; i++)
        {
            if (inventoryItems[i].IsEmpty())
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
