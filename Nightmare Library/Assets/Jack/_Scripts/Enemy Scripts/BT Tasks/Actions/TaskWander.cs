using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TaskWander : Node
{
    private Transform transform;
    private NavMeshAgent navAgent;
    private ActiveAttack owner;

    private bool isWaiting = false;
    private const float pointWaitTimeMin = 1;
    private const float pointWaitTimeMax = 5;
    private float pointWaitTimer = pointWaitTimeMin + (pointWaitTimeMax - pointWaitTimeMin);

    private float wanderSpeed = 7;
    private float wanderAcceleration = 50;

    private Vector3 targetLocation;
    private int targetRing = 0;

    public TaskWander(ActiveAttack owner, NavMeshAgent navAgent)
    {
        transform = navAgent.transform;
        this.navAgent = navAgent;
        this.owner = owner;
    }
    public override Status Check(float dt)
    {
        if (isWaiting)
        {
            pointWaitTimer -= dt;

            if(pointWaitTimer < 0)
            {
                targetLocation = GetNewTarget();
            }
        }
        else
        {
            if (targetLocation == Vector3.zero)
                targetLocation = GetNewTarget();

            if(Vector3.Distance(transform.position, targetLocation) < 3f)
                isWaiting = true;

            navAgent.destination = targetLocation;
            navAgent.speed = wanderSpeed;
            navAgent.acceleration = wanderAcceleration;
        }

        status = Status.RUNNING;
        return status;
    }

    private Vector3 GetNewTarget()
    {
        Debug.Log("Getting New Target");

        isWaiting = false;
        pointWaitTimer = Random.Range(pointWaitTimeMin, pointWaitTimeMax);

        // Assign weighting so that it is more likely that the agent will move to another ring when choosing a target
        List<int> ringWeight = new List<int>();
        for(int i = 0; i < owner.validWanderLocations.Count; i++)
        {
            for(int j = 0; j < Mathf.Abs(targetRing - i) + 1; j++)
            {
                ringWeight.Add(i);
            }
        }

        int randRing = ringWeight[Random.Range(0, ringWeight.Count)];
        int randPoint = Random.Range(0, owner.validWanderLocations[randRing].Count);
        Vector3 point = owner.validWanderLocations[randRing][randPoint];

        Debug.Log(point);
        return point;
    }
}
