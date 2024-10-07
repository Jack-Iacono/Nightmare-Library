using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;

public class TaskRushCooldown : TaskWait
{
    Enemy owner;
    NavMeshAgent navAgent;

    public TaskRushCooldown(float waitTimer, Enemy owner) : base(waitTimer)
    {
        this.owner = owner;
        navAgent = owner.navAgent;
    }

    protected override void OnStart()
    {
        navAgent.destination = navAgent.transform.position;
        Debug.Log("Starting Rush Cooldown");
    }
    protected override void OnEnd()
    {
        OnParentReset();
        parent.SetData(TaskStopRush.COOLDOWN_KEY, false);
    }

    protected override bool TickCondition()
    {
        object temp = GetData(TaskStopRush.COOLDOWN_KEY);
        if (temp != null)
            return (bool)temp;
        return false;
    }
    protected override void OnParentReset()
    {
        base.OnParentReset(); 
    }
}
