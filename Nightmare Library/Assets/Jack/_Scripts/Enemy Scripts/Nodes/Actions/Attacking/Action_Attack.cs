using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public abstract class Action_Attack : Node
{
    protected Enemy enemy;
    protected EnemyPreset preset;

    public Action_Attack(Enemy enemy)
    {
        this.enemy = enemy;
        preset = enemy.enemyType;
    }

    protected void Attack(PlayerController controller)
    {
        if(!EnemyBookController.appliedBooks.ContainsKey(controller) || preset != EnemyBookController.appliedBooks[controller])
        {
            // Kill the player
            controller.ChangeAliveState(false);
        }
        else
        {
            // Eliminate this enemy
            enemy.RemoveEnemy();
        }
        
    }
}
