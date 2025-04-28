using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public abstract class TaskAttack : Node
{
    protected Enemy enemy;
    protected EnemyPreset preset;

    public TaskAttack(Enemy enemy)
    {
        this.enemy = enemy;
        preset = enemy.enemyType;
    }

    protected void Attack(PlayerController controller)
    {
        if(preset != EnemyBookController.appliedBooks[controller])
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
