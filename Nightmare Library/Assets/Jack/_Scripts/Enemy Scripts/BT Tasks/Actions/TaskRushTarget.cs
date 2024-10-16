using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using BehaviorTree;

public class TaskRushTarget : Node
{
    private ActiveAttack owner;
    private NavMeshAgent navAgent;
    private Transform transform;

    private float speed = 50;
    private float acceleration = 1000;

    private bool doInitialize = true;
    private bool isRushing = false;
    private Vector3 previousFramePosition = Vector3.zero;

    Vector3 target;

    public TaskRushTarget(ActiveAttack owner, NavMeshAgent navAgent, float speed = 50, float acceleration = 1000)
    {
        this.owner = owner;
        transform = navAgent.transform;
        this.navAgent = navAgent;
        this.speed = speed;
        this.acceleration = acceleration;
    }

    public override Status Check(float dt)
    {
        // Runs the first time this node is called
        if (doInitialize)
        {
            // Set the settings for the rush
            navAgent.speed = speed;
            navAgent.acceleration = acceleration;

            // Get the current target node
            navAgent.destination = owner.currentTargetStatic;
            previousFramePosition = navAgent.transform.position;
            doInitialize = false;
        }

        if (!isRushing && previousFramePosition != navAgent.transform.position)
            isRushing = true;
        // Check for the ray hitting a wall
        else if (isRushing && previousFramePosition == navAgent.transform.position)
        {
            // Return the navAgent to normal settings
            navAgent.SetDestination(navAgent.transform.position);

            doInitialize = true;
            isRushing = false;

            status = Status.SUCCESS;
            return status;
        }

        previousFramePosition = navAgent.transform.position;

        status = Status.RUNNING;
        return status;
    }
    protected override void OnResetNode()
    {
        base.OnResetNode();
        doInitialize = true;
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
