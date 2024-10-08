using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;

public class TaskRushCooldown : TaskWait
{
    Enemy owner;
    NavMeshAgent navAgent;

    public TaskRushCooldown(float waitTimer, Enemy owner, int resetType = 0) : base(waitTimer, default, resetType)
    {
        this.owner = owner;
        navAgent = owner.navAgent;
    }

    protected override void OnStart()
    {
        navAgent.destination = navAgent.transform.position;
    }
    protected override void OnEnd()
    {
        parent.SetData(TaskStopRush.COOLDOWN_KEY, false);
    }

    protected override bool TickCondition()
    {
        object temp = GetData(TaskStopRush.COOLDOWN_KEY);
        if (temp != null)
            return (bool)temp;
        return false;
    }
}
