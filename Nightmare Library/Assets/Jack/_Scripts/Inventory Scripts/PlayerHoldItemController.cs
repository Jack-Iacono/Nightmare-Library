using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class PlayerHoldItemController : MonoBehaviour
{
    private InventoryController invCont;

    private HoldableItem heldItem;
    [SerializeField]
    private Transform hand;

    public delegate void OnHeldItemChangedDelegate(HoldableItem item);
    public event OnHeldItemChangedDelegate OnHeldItemChanged;

    private void Awake()
    {
        invCont = GetComponent<InventoryController>();
        invCont.OnHeldItemChanged += ChangeHeldItem;
    }

    private void Update()
    {
        if(heldItem != null)
        {
            Vector3 rawOffset = IUseable.Instances.ContainsKey(heldItem.gameObject) ? IUseable.Instances[heldItem.gameObject].GetOffset() : Vector3.zero;
            Vector3 offset =
                rawOffset.x * hand.right +
                rawOffset.y * hand.up +
                rawOffset.z * hand.forward
            ;

            heldItem.trans.position = Vector3.Slerp(heldItem.trans.position, hand.position + offset, 0.2f);
            heldItem.trans.rotation = Quaternion.Slerp(heldItem.trans.rotation, hand.rotation, 0.2f);
        }
        
    }

    public void ChangeHeldItem(InventoryItem item, int change)
    {
        if (change != 2 && heldItem != null)
            heldItem.gameObject.SetActive(false);
        
        if(item != null)
            heldItem = item.holdable;
        else
            heldItem = null;

        OnHeldItemChanged?.Invoke(heldItem);

        if(heldItem != null)
        {
            heldItem.gameObject.SetActive(true);

            if (change == 0)
            {
                heldItem.trans.position = hand.position;
                heldItem.trans.rotation = hand.rotation;
            }
        }
    }
}
