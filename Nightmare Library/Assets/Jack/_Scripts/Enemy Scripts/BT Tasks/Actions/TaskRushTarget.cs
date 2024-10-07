using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using BehaviorTree;
using Unity.VisualScripting;
using static UnityEngine.UI.GridLayoutGroup;

public class TaskRushTarget : Node
{
    public const string RUSH_KEY = "isRushing";

    private NavMeshAgent navAgent;
    private Transform transform;

    private float speedStore = 1;
    private float accelerationStore = 1000;

    private bool doInitialize = true;
    private bool isRushing = false;
    private Vector3 previousFramePosition = Vector3.zero;

    Vector3 target;

    public TaskRushTarget(Transform transform, NavMeshAgent navAgent)
    {
        this.transform = transform;
        this.navAgent = navAgent;

        speedStore = navAgent.speed;
        accelerationStore = navAgent.acceleration;
    }

    public override Status Check(float dt)
    {
        // Runs the first time this node is called
        if (doInitialize)
        {
            // Set the settings for the rush
            navAgent.speed = speedStore * 20;
            navAgent.acceleration = 10000;

            // Get the current target node
            target = (Vector3)GetData(CheckPlayerInSightChase.PLAYER_KEY);

            previousFramePosition = navAgent.transform.position;

            doInitialize = false;
        }

        if (!isRushing && previousFramePosition != navAgent.transform.position)
        {
            //navAgent.acceleration = accelerationStore * 0.5f;
            isRushing = true;
        }
        // Check for the ray hitting a wall
        else if (isRushing && previousFramePosition == navAgent.transform.position)
        {
            // Return the navAgent to normal settings
            navAgent.acceleration = accelerationStore;
            navAgent.SetDestination(navAgent.transform.position);
            navAgent.speed = speedStore;

            doInitialize = true;
            isRushing = false;

            status = Status.SUCCESS;
            return status;
        }

        previousFramePosition = navAgent.transform.position;

        navAgent.destination = target;
        status = Status.RUNNING;
        return status;
    }

    /// <summary>
    /// Trims the y axis out of a vector3
    /// </summary>
    /// <returns>The given Vector3 without the y component</returns>
    private Vector3 TrimVector(Vector3 org)
    {
        return new Vector3(org.x, 1, org.z);
    }
}
