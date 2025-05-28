using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.GridLayoutGroup;

public class Action_WarpAway : Node
{
    aa_Stalk owner;
    private Transform transform;
    private NavMeshAgent navAgent;

    private float dist = 10;

    private bool passCheck = false;

    private Vector3 targetLocation = Vector3.zero;

    public Action_WarpAway(aa_Stalk owner, NavMeshAgent navAgent, float dist = 2)
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
            navAgent.speed = 0;
            if(owner.currentTargetPlayer != null)
                navAgent.Warp(EnemyNavGraph.GetOutOfSightNode(owner.currentTargetPlayer).position);
            else
                navAgent.Warp(EnemyNavGraph.GetRandomNavPoint().position);

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
