using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;
using UnityEngine.AI;

public class TaskAttackTarget : TaskAttack
{
    private bool hasAttacked = false;
    private aa_Stalk owner;
    private NavMeshAgent agent;

    public TaskAttackTarget(Enemy enemy, aa_Stalk owner) : base(enemy)
    {
        this.agent = enemy.navAgent;
        this.owner = owner;
    }
    public override Status Check(float dt)
    {
        if(!hasAttacked) 
        {
            agent.speed = 0;
            Attack(owner.currentTargetPlayer);
            hasAttacked = true;
        }

        status = Status.SUCCESS;
        return status;
    }
    protected override void OnResetNode()
    {
        base.OnResetNode();
        hasAttacked = false;
    }
}
