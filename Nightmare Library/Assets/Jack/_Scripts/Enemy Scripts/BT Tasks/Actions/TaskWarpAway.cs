using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TaskWarpAway : Node
{
    private Transform transform;
    private NavMeshAgent navAgent;

    private float dist = 10;

    private bool passCheck = false;

    private Vector3 targetLocation = Vector3.zero;

    public TaskWarpAway(NavMeshAgent navAgent, float dist = 2)
    {
        this.navAgent = navAgent;
        this.dist = dist;
        transform = navAgent.transform;
    }
    public override Status Check(float dt)
    {
        if (!passCheck)
        {
            targetLocation = EnemyNavGraph.GetFarthestNavPoint(transform.position).position;

            navAgent.speed = 0;
            navAgent.Warp(targetLocation);

            passCheck = true;
        }

        status = Status.SUCCESS;
        return status;
    }

    protected override void OnResetNode()
    {
        base.OnResetNode();
        passCheck = false;
    }
}
