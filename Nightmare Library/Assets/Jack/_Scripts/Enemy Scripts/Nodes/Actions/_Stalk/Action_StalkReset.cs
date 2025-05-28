using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Action_StalkReset : Node
{
    private aa_Stalk owner;

    public Action_StalkReset(aa_Stalk owner)
    {
        this.owner = owner;
    }
    public override Status Check(float dt)
    {
        Debug.Log("Reset Stalk");

        owner.RemoveTarget();

        status = Status.SUCCESS;
        return status;
    }
}
