using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Node_StalkRemoveTarget : Node
{
    private aa_Stalk owner;

    public Node_StalkRemoveTarget(aa_Stalk owner)
    {
        this.owner = owner;
    }
    public override Status Check(float dt)
    {
        owner.RemoveTarget();

        status = Status.SUCCESS;
        return status;
    }
}
