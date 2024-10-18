using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;
using UnityEngine.AI;

public class TaskAttackPlayersInRange : Node
{
    private bool hasAttacked = false;

    private Transform transform;
    private NavMeshAgent agent;
    private float range = 5;

    public TaskAttackPlayersInRange(NavMeshAgent agent, float range = 5)
    {
        this.agent = agent;
        this.range = range;
        transform = agent.transform;
    }
    public override Status Check(float dt)
    {
        if (!hasAttacked)
        {
            agent.speed = 0;

            foreach (PlayerController p in PlayerController.playerInstances)
            {
                if(Vector3.Distance(p.transform.position, transform.position) <= range)
                {
                    Debug.Log("Attack " + p.name);
                }
            }
            
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
