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
    public Transform currentTarget { get; protected set; } = null;

    public ActiveAttack(Enemy owner)
    {
        this.owner = owner;
    }

    public virtual void OnDestroy()
    {

    }
}
