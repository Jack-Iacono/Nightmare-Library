using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using BehaviorTree;

public class TaskRushTarget : Node
{
    private NavMeshAgent navAgent;
    private Transform transform;

    private float speedStore = 1;
    private float accelerationStore = 1000;

    public TaskRushTarget(Transform transform, NavMeshAgent navAgent)
    {
        this.transform = transform;
        this.navAgent = navAgent;

        speedStore = navAgent.speed;
        accelerationStore = navAgent.acceleration;
    }

    public override Status Check(float dt)
    {
        // Get the current target node
        Vector3 target = (Vector3)GetData(CheckPlayerInSightChase.PLAYER_KEY);

        if(navAgent.pathStatus == NavMeshPathStatus.PathComplete)
        {
            // Check if the agent is still not at the target
            if (Vector3.Distance(transform.position, target) > 0.5f)
            {
                navAgent.speed = speedStore * 10;

                navAgent.destination = target;
                status = Status.RUNNING;
                return status;
            }
        }

        navAgent.speed = speedStore;

        status = Status.SUCCESS;
        return status;
    }
}
