using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.GridLayoutGroup;

public class TaskWarpAway : Node
{
    aa_Stalk owner;
    private Transform transform;
    private NavMeshAgent navAgent;

    private float dist = 10;

    private bool passCheck = false;

    private Vector3 targetLocation = Vector3.zero;

    public TaskWarpAway(aa_Stalk owner, NavMeshAgent navAgent, float dist = 2)
    {
        this.navAgent = navAgent;
        this.dist = dist;
        this.owner = owner;
        transform = navAgent.transform;
    }
    public override Status Check(float dt)
    {
        if (!passCheck)
        {
            targetLocation = EnemyNavGraph.GetFarthestNavPoint(transform.position).position;

            navAgent.speed = 0;
            navAgent.Warp(targetLocation);

            owner.UseStalkAttempt();

            passCheck = true;
        }

        status = Status.SUCCESS;
        return status;
    }

    protected override void OnResetNode()
    {
        base.OnResetNode();
        passCheck = false;
    }
}
