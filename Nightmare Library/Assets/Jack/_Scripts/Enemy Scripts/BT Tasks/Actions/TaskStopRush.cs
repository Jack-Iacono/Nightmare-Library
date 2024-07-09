using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;
using UnityEngine.AI;

public class TaskStopRush : Node
{
    public const string RUSH_KEY = TaskStartRush.RUSH_KEY;
    public const string COOLDOWN_KEY = "RushCooldown";

    Enemy owner;
    NavMeshAgent navAgent;

    public TaskStopRush(Enemy owner)
    {
        this.owner = owner;
        navAgent = owner.navAgent;
    }

    public override Status Check(float dt)
    {
        // Set up the enemy for the rush
        parent.parent.SetData(COOLDOWN_KEY, true);
        parent.parent.SetData(RUSH_KEY, false);
        parent.parent.ClearData(CheckPlayerInSightChase.PLAYER_KEY);

        Debug.Log("Rush Stopped");

        navAgent.acceleration = 100;
        navAgent.SetDestination(owner.transform.position);

        status = Status.SUCCESS;
        return status;
    }
}
