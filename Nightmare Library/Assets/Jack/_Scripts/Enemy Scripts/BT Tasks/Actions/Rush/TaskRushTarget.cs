using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using BehaviorTree;
using System.IO;

public class TaskRushTarget : Node
{
    private aa_Rush owner;
    private NavMeshAgent navAgent;
    private Transform transform;

    private float speed = 50;

    private float nodeWaitTime = 0.25f;
    private float nodeWaitTimer = 0.25f;

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
            currentTargetNode = GetNextNodeInPath();

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
            navAgent.speed = speed;
            navAgent.acceleration = speed * 5;

            // Is the player close enough to the target node
            if (Vector3.SqrMagnitude(transform.position - currentTargetNode.position) < 9)
            {
                atNodeWait = true;
                currentTargetNode = GetNextNodeInPath();
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

    public void OnLevelChange(float nodeWaitTime, float speed)
    {
        this.nodeWaitTime = nodeWaitTime;
        this.speed = speed;
    }

    public EnemyNavNode GetNextNodeInPath()
    {
        if (owner.path.Count > 0)
        {
            EnemyNavNode node = owner.path[0];
            owner.path.RemoveAt(0);
            return node;
        }
        return null;
    }
}
