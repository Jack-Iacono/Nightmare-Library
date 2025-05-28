using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Check_ConditionNewTarget : Check_Condition
{
    // Does this node know about the target yet
    private bool targetKnown = false;
    private bool passCheck = false;

    private NavMeshAgent agent;
    private ActiveAttack owner;

    public Check_ConditionNewTarget(ActiveAttack owner, Enemy enemy) : base()
    {
        this.owner = owner;
        agent = enemy.navAgent;
    }
    protected override bool EvaluateCondition()
    {
        // If there is no target and the node is not already running
        if (owner.currentTargetStatic == Vector3.zero && !passCheck)
        {
            targetKnown = false;
        }
        else if(!targetKnown)
        {
            targetKnown = true;
            passCheck = true;
        }

        return targetKnown && passCheck;
    }
    protected override void OnPass()
    {
        agent.speed = 0;
        agent.acceleration = 500;
    }
    protected override void OnPassTick()
    {
        agent.speed = 0;
        agent.acceleration = 500;
    }
    protected override void OnResetNode()
    {
        // Reset on success so that the timer no longer runs
        passCheck = false;
        base.OnResetNode();
    }
}
