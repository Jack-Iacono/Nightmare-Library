using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;
using UnityEngine.AI;

public class TaskPatrol : Node
{
    private Transform transform;
    private List<EnemyNavPoint> navPoints;
    private NavMeshAgent navAgent;

    private int currentNavPointIndex = 0;

    private float waitTime = 0.5f;
    private float waitTimer = 0f;
    private bool waiting = false;

    public TaskPatrol(Transform transform, List<EnemyNavPoint> navPoints, NavMeshAgent navAgent)
    {
        this.transform = transform;
        this.navPoints = navPoints;
        this.navAgent = navAgent;
    }
    public override Status Check(float dt)
    {
        // check if the agent is waiting
        if (waiting)
        {
            // Increment the timer
            waitTimer += Time.deltaTime;

            // Check if the timer is still above 0
            if (waitTimer < waitTime)
                return Status.RUNNING;

            // Set the agent to stop waiting
            waiting = false;
        }

        // Get the waypoint that the agent should go to
        Vector3 point = navPoints[currentNavPointIndex].position;

        // Check if the agent is not at the destination yet
        if(Vector3.Distance(transform.position, point) < 0.5f)
        {
            waitTimer = 0;
            waiting = true;

            // Switch to the next waypoint
            currentNavPointIndex = (currentNavPointIndex + 1) % navPoints.Count;
        }
        else
        {
            // Make the agent go to the waypoint
            navAgent.destination = point;
            navAgent.speed = (float)GetData("speed");
        }

        status = Status.RUNNING;
        return status;
    }
}
