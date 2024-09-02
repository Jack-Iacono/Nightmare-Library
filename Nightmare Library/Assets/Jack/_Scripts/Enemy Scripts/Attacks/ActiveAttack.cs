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

    public ActiveAttack(Enemy owner)
    {
        this.owner = owner;
    }

    public virtual void OnDestroy()
    {

    }
}
