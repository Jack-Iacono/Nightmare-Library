using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PassiveAttack : EnemyAttack
{
    public PassiveAttack(Enemy owner) : base(owner)
    {

    }
}
