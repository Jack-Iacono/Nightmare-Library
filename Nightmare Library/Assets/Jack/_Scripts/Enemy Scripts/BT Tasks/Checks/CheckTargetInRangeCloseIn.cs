using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;
using static UnityEngine.UI.GridLayoutGroup;
using UnityEngine.AI;

public class CheckTargetInRangeCloseIn : CheckTargetInRange
{
    private NavMeshAgent navAgent;

    public CheckTargetInRangeCloseIn(ActiveAttack owner, NavMeshAgent navAgent, float range = 10) : base(owner, navAgent.transform, range)
    {
        this.navAgent = navAgent;
    }

    protected override void InRangeAction()
    {
        navAgent.speed = 0;
        Debug.Log("Make Noise");
    }
}
