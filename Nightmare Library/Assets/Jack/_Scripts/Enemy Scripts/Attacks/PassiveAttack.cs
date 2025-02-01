using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PassiveAttack : Attack
{
    public PassiveAttack(Enemy owner)
    {
        this.owner = owner;
        GameController.OnLevelChange += OnLevelChange;
        currentLevel = startingLevel;
    }

    public Enemy GetOwner()
    {
        return owner;
    }
}
