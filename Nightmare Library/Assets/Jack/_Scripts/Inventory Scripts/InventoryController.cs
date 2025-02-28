using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public static InventoryController instance;

    private const int inventorySize = 3;
    private InventoryItem[] inventoryItems = new InventoryItem[inventorySize];

    private int currentItemIndex = 0;

    public static KeyCode keyInventoryUp = KeyCode.Z;
    public static KeyCode keyInventoryDown = KeyCode.X;

    public delegate void OnHeldItemChangedDelegate(InventoryItem heldObject);
    public event OnHeldItemChangedDelegate onHeldItemChanged;

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
        onHeldItemChanged?.Invoke(inventoryItems[currentItemIndex]);
    }

    private void Update()
    {
        if (!GameController.gamePaused)
        {
            if (Input.GetKeyDown(keyInventoryUp))
            {
                currentItemIndex = (currentItemIndex + 1) % inventorySize;
                onHeldItemChanged?.Invoke(inventoryItems[currentItemIndex]);
            }
            else if (Input.GetKeyDown(keyInventoryDown))
            {
                currentItemIndex = (currentItemIndex - 1 + inventorySize) % inventorySize;
                onHeldItemChanged?.Invoke(inventoryItems[currentItemIndex]);
            }
        }
    }

    public InventoryItem GetCurrentItem()
    {
        return inventoryItems[currentItemIndex];
    }
    public bool AddItem(GameObject item)
    {
        int open = GetOpenInventorySlot();

        if (open != -1)
        {
            inventoryItems[open].Set(item);
            currentItemIndex = open;
        }
        else
            return false;

        onHeldItemChanged?.Invoke(inventoryItems[currentItemIndex]);

        return true;
    }
    public bool RemoveCurrentItem()
    {
        if (!inventoryItems[currentItemIndex].isEmpty)
        {
            inventoryItems[currentItemIndex].Clear();
            onHeldItemChanged?.Invoke(inventoryItems[currentItemIndex]);
            return true;
        }

        return false;
    }

    public InventoryItem[] GetInventoryItems()
    {
        return inventoryItems;
    }
    public void ClearInventory()
    {
        foreach(InventoryItem item in inventoryItems)
        {
            item.Clear();
        }

        currentItemIndex = 0;
        onHeldItemChanged?.Invoke(inventoryItems[currentItemIndex]);
    }
    
    public bool HasOpenSlot()
    {
        return GetOpenInventorySlot() != -1;
    }
    private int GetOpenInventorySlot()
    {
        for(int i = 0; i < inventoryItems.Length; i++)
        {
            if (inventoryItems[i].isEmpty)
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
