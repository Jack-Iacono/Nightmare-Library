using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;
using Unity.VisualScripting;
using UnityEngine.AI;

public class TaskAssignStalkTarget : Node
{
    private aa_Stalk owner;

    public TaskAssignStalkTarget(aa_Stalk owner)
    {
        this.owner = owner;
    }
    public override Status Check(float dt)
    {
        // Makes the owner enter it's stalking phase
        if (owner.BeginStalking())
        {
            Debug.Log("Assigned Target");

            status = Status.SUCCESS;
            return status;
        }

        Debug.Log("Failed Target Assignment");
        status = Status.FAILURE;
        return status;
    }
}
