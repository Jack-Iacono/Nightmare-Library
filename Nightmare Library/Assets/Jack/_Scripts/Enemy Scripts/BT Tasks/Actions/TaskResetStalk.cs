using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TaskResetStalk : Node
{
    private aa_Stalk owner;

    public TaskResetStalk(aa_Stalk owner)
    {
        this.owner = owner;
    }
    public override Status Check(float dt)
    {
        owner.EmptyStalkAttempts();
        owner.RemoveTarget();

        status = Status.SUCCESS;
        return status;
    }
}
