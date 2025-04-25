using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItem
{
    public HoldableItem holdable;
    public IUseable useable;

    public InventoryItem()
    {
        holdable = null;
        useable = null;
    }
    public InventoryItem(HoldableItem h)
    {
        holdable = h;
        if (IUseable.Instances.ContainsKey(h.gameObject))
            useable = IUseable.Instances[h.gameObject];
        else
            useable = null;
    }

    public void Set(HoldableItem h)
    {
        holdable = h;
        if (IUseable.Instances.ContainsKey(h.gameObject))
            useable = IUseable.Instances[h.gameObject];
        else
            useable = null;
    }
    public void Clear()
    {
        holdable = null;
        useable = null;
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
