using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUsableController : HoldableItem, IUseable
{
    protected override void Awake()
    {
        base.Awake();
        IUseable.Instances.Add(gameObject, this);
    }

    public void Use()
    {
        Debug.Log("Click");
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        IUseable.Instances.Remove(gameObject);
    }
}
