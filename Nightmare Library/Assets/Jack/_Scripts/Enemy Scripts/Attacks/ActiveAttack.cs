using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class ActiveAttack : BehaviorTree.Tree
{
    public string name;
    public string toolTip;

    protected Enemy owner;

    // These variables are used among almost all attacks
    public Transform currentTargetDynamic { get; protected set; } = null;
    public Vector3 currentTargetStatic { get; protected set; } = Vector3.zero;
    public static readonly LayerMask envLayers = 1 << 9 | 1 << 2;

    public ActiveAttack(Enemy owner)
    {
        this.owner = owner;
    }

    public void SetCurrentTarget(Transform t)
    {
        currentTargetDynamic = t;
        if(t != null) 
            currentTargetStatic = t.position;
        else
            currentTargetStatic = Vector3.zero;
    }
    public void SetCurrentTarget(Vector3 position)
    {
        currentTargetStatic = position;
    }

    public virtual void OnDestroy()
    {

    }
}
