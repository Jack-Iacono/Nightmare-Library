using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;
using Unity.VisualScripting;
using UnityEngine.AI;

public class TaskWanderTimed : Node
{
    private Transform transform;
    private NavMeshAgent navAgent;
    private aa_Stalk owner;

    private float wanderTimeMin;
    private float wanderTimeMax;
    private float wanderTimer;

    private bool isWaiting = false;
    private const float pointWaitTimeMin = 1;
    private const float pointWaitTimeMax = 5;
    private float pointWaitTimer = pointWaitTimeMin + (pointWaitTimeMax - pointWaitTimeMin);

    private float wanderSpeed = 7;
    private float wanderAcceleration = 50;

    private EnemyNavNode currentNavPoint = null;
    private Vector3 targetLocation;

    public TaskWanderTimed(aa_Stalk owner, NavMeshAgent navAgent, float wanderTimeMin = 10, float wanderTimeMax = 20)
    {
        transform = navAgent.transform;
        this.navAgent = navAgent;
        this.wanderTimeMin = wanderTimeMin;
        this.wanderTimeMax = wanderTimeMax;
        this.owner = owner;
        wanderTimer = Random.Range(wanderTimeMin, wanderTimeMax);
    }
    public override Status Check(float dt)
    {
        // Timer for how long the enemy should wander for, does not increment if no players are out of the office
        if (wanderTimer > 0)
            wanderTimer -= DeskController.playersAtDesk.Count < PlayerController.playerInstances.Count ? dt : 0;
        else
        {
            wanderTimer = Random.Range(wanderTimeMin, wanderTimeMax);
            status = Status.SUCCESS;
            return status;
        }

        if (!isWaiting)
        {
            // Check to make sure that the agent has a target
            if (currentNavPoint == null)
                GetNewTarget();

            // Check if the player is close enough to the wander point
            if (Vector3.SqrMagnitude(transform.position - targetLocation) > 1f)
            {
                // Ensure the agent is going to the correct location
                if (navAgent.destination != targetLocation)
                    navAgent.destination = targetLocation;

                // Ensure the agent is going at the correct speed
                if (navAgent.speed != wanderSpeed)
                {
                    navAgent.speed = wanderSpeed;
                    navAgent.acceleration = wanderAcceleration;
                }
            }
            else
                isWaiting = true;
        }
        else
        {
            if(pointWaitTimer > 0)
                pointWaitTimer -= dt;
            else
                GetNewTarget();
        }

        status = Status.RUNNING;
        return status;
    }

    private void GetNewTarget()
    {
        currentNavPoint = EnemyNavGraph.GetRandomNavPoint();
        targetLocation = currentNavPoint.position;

        navAgent.destination = targetLocation;
        navAgent.speed = wanderSpeed;
        navAgent.acceleration = wanderAcceleration;

        isWaiting = false;
        pointWaitTimer = Random.Range(pointWaitTimeMin, pointWaitTimeMax);
    }

    public void OnLevelChange(float min, float max)
    {
        wanderTimeMin = min;
        wanderTimeMax = max;
    }
}
