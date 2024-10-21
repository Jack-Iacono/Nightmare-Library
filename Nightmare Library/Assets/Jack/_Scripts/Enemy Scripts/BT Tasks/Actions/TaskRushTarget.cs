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

    private List<EnemyNavPoint> path;

    private float nodeWaitTime = 1;
    private float nodeWaitTimer = 1;

    private bool pathFinished = false;
    private bool atNodeWait = false;

    private EnemyNavPoint currentGoal;
    private EnemyNavPoint previousGoal;

    public TaskRushTarget(ActiveAttack owner, NavMeshAgent navAgent, float nodeWaitTime = 1, float speed = 50)
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
        if(path == null)
            GetNewPath();

        if (pathFinished)
        {
            status = Status.SUCCESS;
            return status;
        }

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
        else if (path.Count > 0)
        {
            navAgent.destination = path[0].position;
            navAgent.speed = speed;

            // Is the player close enough to the point in the beginning of the list
            if (Vector3.SqrMagnitude(transform.position - path[0].position) < 1)
            {
                atNodeWait = true;
                path.RemoveAt(0);
            }
        }
        else
        {
            // Get a new path and return success;
            pathFinished = true;
            GetNewPath();
        }

        status = Status.RUNNING;
        return status;
    }
    protected override void OnResetNode()
    {
        base.OnResetNode();
        pathFinished = false;
    }

    private void GetNewPath()
    {
        if (currentGoal == null)
            currentGoal = EnemyNavGraph.GetClosestNavPoint(transform.position);

        previousGoal = currentGoal;
        currentGoal = EnemyNavGraph.GetRandomNavPoint();

        path = EnemyNavGraph.GetPathToPoint(previousGoal, currentGoal);
    }
}
