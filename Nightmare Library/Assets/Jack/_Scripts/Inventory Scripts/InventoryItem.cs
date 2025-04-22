using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItem
{
    private bool isEmpty = true;

    public HoldableItem holdable;
    public IUseable useable;

    public InventoryItem()
    {
        holdable = null;
        useable = null;
        isEmpty = true;
    }
    public InventoryItem(HoldableItem h)
    {
        holdable = h;
        if (IUseable.Instances.ContainsKey(h.gameObject))
            useable = IUseable.Instances[h.gameObject];
        else
            useable = null;
        isEmpty = false;
    }

    public void Set(HoldableItem h)
    {
        holdable = h;
        if (IUseable.Instances.ContainsKey(h.gameObject))
            useable = IUseable.Instances[h.gameObject];
        else
            useable = null;
        isEmpty = false;
    }
    public void Clear()
    {
        holdable = null;
        useable = null;
        isEmpty = true;
    }

    public bool IsEmpty()
    {
        return isEmpty; 
    }

    public bool Equals(HoldableItem item)
    {
        return holdable == item;
    }
    public bool Equals(InventoryItem other)
    {
        return other.holdable == holdable;
    }
}
