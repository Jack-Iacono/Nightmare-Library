using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;

[RequireComponent(typeof(PlayerInteractionController))]
public class InventoryController : MonoBehaviour
{
    private PlayerInteractionController interactionController;

    private const int inventorySize = 3;
    private InventoryItem[] inventoryItems = new InventoryItem[inventorySize];

    private int currentItemIndex = 0;

    public static KeyCode keyInventoryUp = KeyCode.Z;
    public static KeyCode keyInventoryDown = KeyCode.X;

    public static InventoryItem currentHeldItem = null;

    /// <summary>
    /// Alerts other scripts that the current held item is being changed
    /// </summary>
    /// <param name="current">The currently held item</param>
    /// <param name="changeType">0: Change Held Item\n1: Pickup</param>
    public delegate void OnHeldItemChangedDelegate(InventoryItem current, int changeType = 0);
    public event OnHeldItemChangedDelegate OnHeldItemChanged;

    private void Awake()
    {
        interactionController = GetComponent<PlayerInteractionController>();

        for (int i = 0; i < inventorySize; i++)
        {
            inventoryItems[i] = null;
        }
    }
    private void Start()
    {
        if (PlayerController.playerInstances[gameObject] != PlayerController.mainPlayerInstance)
            enabled = false;
        else
        {
            SetCurrentIndex(0);
        }
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

        CheckHeldItem(1);

        return true;
    }
    public bool RemoveCurrentItem()
    {
        if (inventoryItems[currentItemIndex] != null)
        {
            inventoryItems[currentItemIndex] = null;
            currentHeldItem = null;

            CheckHeldItem(2);

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
            if (inventoryItems[i] != null)
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
        CheckHeldItem(0);
    }

    private void SetCurrentIndex(int index)
    {
        currentItemIndex = index;
        currentHeldItem = inventoryItems[currentItemIndex];

        CheckHeldItem(0);
    }

    private void CheckHeldItem(int changeType = 0)
    {
        OnHeldItemChanged?.Invoke(currentHeldItem, changeType);
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
}
