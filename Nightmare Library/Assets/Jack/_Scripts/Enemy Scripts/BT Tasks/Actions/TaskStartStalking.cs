using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;
using Unity.VisualScripting;
using UnityEngine.AI;

public class TaskStartStalking : Node
{
    private aa_Stalk owner;

    public TaskStartStalking(aa_Stalk owner)
    {
        this.owner = owner;
    }
    public override Status Check(float dt)
    {
        // Makes the owner enter it's stalking phase
        owner.BeginStalking();

        status = Status.SUCCESS;
        return status;
    }
}
