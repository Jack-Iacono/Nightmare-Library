using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public static InventoryController instance;

    private const int inventorySize = 3;
    private HoldableItem[] inventoryItems = new HoldableItem[inventorySize];

    private int currentItemIndex = 0;

    public static KeyCode keyInventoryUp = KeyCode.Z;
    public static KeyCode keyInventoryDown = KeyCode.X;

    public static HoldableItem currentHeldItem = null;

    public delegate void OnHeldItemChangedDelegate(HoldableItem holdable);
    public event OnHeldItemChangedDelegate onHeldItemChanged;

    private void Awake()
    {
        if(instance != null)
            Destroy(instance.gameObject);
        instance = this;

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
        if (!GameController.gamePaused)
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
        if (!inventoryItems.Contains(moveable))
        {
            for(int i = 0; i < inventorySize; i++)
            {
                if (inventoryItems[i] == null)
                {
                    inventoryItems[i] = moveable;
                    break;
                }
            }
        }
    }

    public HoldableItem GetCurrentItem()
    {
        return currentHeldItem;
    }
    public bool AddItem(HoldableItem item)
    {
        int open = GetOpenInventorySlot();

        if (open != -1)
        {
            inventoryItems[open] = item;
            currentItemIndex = open;
        }
        else
            return false;

        currentHeldItem = inventoryItems[currentItemIndex];
        onHeldItemChanged?.Invoke(inventoryItems[currentItemIndex]);

        return true;
    }
    public bool RemoveCurrentItem()
    {
        if (inventoryItems[currentItemIndex] != null)
        {
            inventoryItems[currentItemIndex] = null;
            onHeldItemChanged?.Invoke(inventoryItems[currentItemIndex]);
            currentHeldItem = null;
            return true;
        }

        return false;
    }

    public HoldableItem[] GetInventoryItems()
    {
        return inventoryItems;
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
        onHeldItemChanged?.Invoke(currentHeldItem);
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
