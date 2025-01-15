using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using BehaviorTree;

public class TaskRushTarget : Node
{
    private aa_Rush owner;
    private NavMeshAgent navAgent;
    private Transform transform;

    private float speed = 50;

    private float nodeWaitTime = 1;
    private float nodeWaitTimer = 1;

    private bool atNodeWait = false;

    private EnemyNavNode currentTargetNode = null;

    public TaskRushTarget(aa_Rush owner, NavMeshAgent navAgent, float nodeWaitTime = 1, float speed = 150)
    {
        this.owner = owner;
        transform = navAgent.transform;
        this.navAgent = navAgent;
        this.speed = speed;

        this.nodeWaitTime = nodeWaitTime;
        nodeWaitTimer = nodeWaitTime;   
    }

    public override Status Check(float dt)
    {
        if (currentTargetNode == null)
            currentTargetNode = owner.GetNextNode();

        if (atNodeWait)
        {
            if (nodeWaitTimer > 0)
                nodeWaitTimer -= dt;
            else
            {
                nodeWaitTimer = nodeWaitTime;
                atNodeWait = false;
            }
        }
        else if (currentTargetNode != null)
        {
            navAgent.destination = currentTargetNode.position;
            navAgent.speed = speed * owner.currentLevel;
            navAgent.acceleration = 1000 * owner.currentLevel;

            // Is the player close enough to the target node
            if (Vector3.SqrMagnitude(transform.position - currentTargetNode.position) < 1)
            {
                atNodeWait = true;
                currentTargetNode = owner.GetNextNode();
            }
        }
        else
        {
            status = Status.SUCCESS;
            return status;
        }

        status = Status.RUNNING;
        return status;
    }
}
