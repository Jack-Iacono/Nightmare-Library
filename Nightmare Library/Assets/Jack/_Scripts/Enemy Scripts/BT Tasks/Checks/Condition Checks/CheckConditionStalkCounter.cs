using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CheckConditionStalkCounter : CheckCondition
{
    protected aa_Stalk owner;
    protected NavMeshAgent agent;

    public CheckConditionStalkCounter(aa_Stalk owner, NavMeshAgent agent) : base()
    {
        this.owner = owner;
        this.agent = agent;
    }

    protected override bool EvaluateCondition()
    {
        return owner.stalkAttemptCounter > 0;
    }
    protected override void OnPass()
    {
        agent.speed = 0;
    }

    protected override void OnResetNode()
    {
        base.OnResetNode();
    }
}
