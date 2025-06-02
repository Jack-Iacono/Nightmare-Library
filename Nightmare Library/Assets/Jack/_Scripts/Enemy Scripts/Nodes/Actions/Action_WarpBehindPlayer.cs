 using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Action_WarpBehindPlayer : Node
{
    private NavMeshAgent navAgent;
    private Transform transform;
    private Enemy enemy;

    private bool hasWarped = false;

    private AudioClip warpSound;

    public delegate PlayerController WarpBehindTargetDelegate();

    // This is used to dynamically get the player regardless of the parent's class type
    private WarpBehindTargetDelegate GetTarget;

    public Action_WarpBehindPlayer(Enemy enemy, WarpBehindTargetDelegate playerDelegate)
    {
        this.enemy = enemy;
        navAgent = enemy.navAgent;
        transform = navAgent.transform;

        GetTarget = playerDelegate;
    }
    public override Status Check(float dt)
    {
        if (!hasWarped)
        {
            if(GetTarget() != null)
                navAgent.Warp(EnemyNavGraph.GetOutOfSightNode(GetTarget()).position);
            navAgent.speed = 0;
            hasWarped = true;
        }

        status = Status.SUCCESS;
        return status;
    }

    protected override void OnResetNode()
    {
        hasWarped = false;
        base.OnResetNode();
    }
}
