using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TaskRunAway : Node
{
    private Transform transform;
    private NavMeshAgent navAgent;

    private float dist = 10;
    private float speed = 30;

    private bool passCheck = false;

    private bool hasTarget = false;
    private Vector3 targetLocation = Vector3.zero;

    public TaskRunAway(NavMeshAgent navAgent, float dist = 2)
    {
        this.navAgent = navAgent;
        this.dist = dist;
        transform = navAgent.transform;
    }
    public override Status Check(float dt)
    {
        if (!passCheck)
        {
            targetLocation = EnemyNavPointController.GetFarthestNavPoint(transform.position).position;

            navAgent.speed = 0;
            navAgent.Warp(targetLocation);

            hasTarget = false;
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
