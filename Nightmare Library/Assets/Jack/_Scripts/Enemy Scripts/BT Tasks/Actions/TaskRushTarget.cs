using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using BehaviorTree;
using System.Buffers;

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
        Debug.Log("Rushing Target");

        // Get the current target node
        Vector3 target = (Vector3)GetData(CheckPlayerInSightChase.PLAYER_KEY);

        // Check if the agent is still not at the target
        if (Vector3.Distance(transform.position, target) > 0.5f)
        {
            navAgent.speed = speedStore * 3;

            navAgent.destination = target;
            status = Status.RUNNING;
            return status;
        }

        navAgent.speed = speedStore;

        status = Status.SUCCESS;
        return status;
    }
}
