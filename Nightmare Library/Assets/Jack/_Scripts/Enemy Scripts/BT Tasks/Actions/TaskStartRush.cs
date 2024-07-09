using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;
using UnityEngine.AI;

public class TaskStartRush : Node
{
    public const string RUSH_KEY = "isRushing";

    Enemy owner;
    NavMeshAgent navAgent;

    public TaskStartRush(Enemy owner) 
    {
        this.owner = owner;
        navAgent = owner.navAgent;
    }

    public override Status Check(float dt)
    {
        // Set up the enemy for the rush
        parent.parent.SetData(RUSH_KEY, true);

        navAgent.speed = 10;

        status = Status.SUCCESS;
        return status;
    }
}
