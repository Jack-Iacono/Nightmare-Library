using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;
using UnityEngine.AI;

public class TaskAttackTarget : Node
{
    private bool hasAttacked = false;
    private NavMeshAgent agent;

    public TaskAttackTarget(NavMeshAgent agent)
    {
        this.agent = agent;
    }
    public override Status Check(float dt)
    {
        if(hasAttacked) 
        {
            agent.speed = 0;
            Debug.Log("Attack Player");
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
