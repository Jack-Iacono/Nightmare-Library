using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActiveAttack : EnemyAttack
{
    public ActiveAttack(Enemy owner) : base(owner)
    {

    }

}
