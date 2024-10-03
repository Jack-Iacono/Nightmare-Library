using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;
using UnityEngine.AI;

public class TaskStopRush : Node
{
    public const string RUSH_KEY = TaskRushTarget.RUSH_KEY;
    public const string COOLDOWN_KEY = "RushCooldown";

    Enemy owner;

    public TaskStopRush(Enemy owner)
    {
        this.owner = owner;
    }

    public override Status Check(float dt)
    {
        // Set up the enemy for the rush
        parent.parent.SetData(COOLDOWN_KEY, true);
        parent.parent.SetData(RUSH_KEY, false);
        parent.parent.ClearData(CheckPlayerInSightChase.PLAYER_KEY);

        status = Status.SUCCESS;
        return status;
    }
}
