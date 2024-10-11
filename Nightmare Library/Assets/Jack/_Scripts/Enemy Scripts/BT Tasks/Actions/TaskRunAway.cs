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
        if(!hasTarget)
        {
            hasTarget = true;
            targetLocation = EnemyNavPointController.GetFarthestNavPoint(transform.position).position;
        }

        // Check if the player is close enough to the wander point
        if (Vector3.SqrMagnitude(transform.position - targetLocation) > dist * dist)
        {
            // Ensure the agent is going to the correct location
            if (navAgent.destination != targetLocation)
                navAgent.destination = targetLocation;

            // Ensure the agent is going at the correct speed
            if (navAgent.speed != speed)
                navAgent.speed = speed;

            status = Status.RUNNING;
            return status;
        }

        hasTarget = false;

        status = Status.SUCCESS;
        return status;
    }

    protected override void OnResetNode()
    {
        base.OnResetNode();
        passCheck = false;
    }
}
