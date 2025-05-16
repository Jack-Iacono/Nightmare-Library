using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UsableController : HoldableItem, IUseable
{
    [Header("Useable Variables")]
    [SerializeField]
    private Vector3 holdOffset = Vector3.zero;

    [SerializeField]
    private ParticleSystem pSystem;

    protected override void Awake()
    {
        base.Awake();
        IUseable.Instances.Add(gameObject, this);
    }

    public void Use()
    {
        pSystem.Play();
    }
    public Vector3 GetOffset()
    {
        return holdOffset;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        IUseable.Instances.Remove(gameObject);
    }
}
