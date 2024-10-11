using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;
using static UnityEngine.UI.GridLayoutGroup;
using UnityEngine.AI;

public class CheckStalkCloseIn : Node
{
    private Transform transform;
    private NavMeshAgent navAgent;
    private aa_Stalk owner;

    private float dist = 10;

    private bool passCheck = false;

    public CheckStalkCloseIn(aa_Stalk owner, NavMeshAgent navAgent, float dist = 10)
    {
        this.owner = owner;
        this.navAgent = navAgent;
        this.dist = dist;
        transform = navAgent.transform;
    }
    public override Status Check(float dt)
    {
        if (passCheck || (owner.currentTarget != null && Vector3.SqrMagnitude(transform.position - owner.currentTarget.transform.position) < dist * dist))
        {
            if (!passCheck)
            {
                navAgent.speed = 0;
                owner.UseStalkAttempt();
            }

            passCheck = true;

            status = Status.SUCCESS;
            return status;
        }

        status = Status.FAILURE;
        return status;
    }

    protected override void OnResetNode()
    {
        base.OnResetNode();
        passCheck = false;
    }
}
