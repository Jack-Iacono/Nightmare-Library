using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItem
{
    public bool isEmpty { get; private set; } = true;
    public GameObject realObject;

    public InventoryItem()
    {
        realObject = null;
        isEmpty = true;
    }
    public InventoryItem(GameObject g)
    {
        realObject = g;
        isEmpty = false;
    }

    public void Set(GameObject g)
    {
        realObject = g;
        isEmpty = false;
    }
    public void Clear()
    {
        realObject = null;
        isEmpty = true;
    }
}
