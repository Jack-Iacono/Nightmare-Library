using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;
using UnityEngine.AI;

public class TaskAttackTarget : Node
{
    private bool hasAttacked = false;
    private aa_Stalk owner;
    private NavMeshAgent agent;

    public TaskAttackTarget(aa_Stalk owner, NavMeshAgent agent)
    {
        this.agent = agent;
        this.owner = owner;
    }
    public override Status Check(float dt)
    {
        if(!hasAttacked) 
        {
            agent.speed = 0;
            owner.currentTargetPlayer.ChangeAliveState(false);
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
