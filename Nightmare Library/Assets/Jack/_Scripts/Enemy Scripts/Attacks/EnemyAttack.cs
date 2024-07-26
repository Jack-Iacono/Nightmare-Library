using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;
using UnityEngine.AI;

public abstract class EnemyAttack : BehaviorTree.Tree
{
    public string name;
    public string toolTip;

    protected Enemy owner;

    public EnemyAttack(Enemy owner)
    {
        this.owner = owner;
    }

    public virtual void OnDestroy()
    {

    }
}
