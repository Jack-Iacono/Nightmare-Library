using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;
using UnityEngine.AI;

public class TaskStartRush : Node
{
    private aa_RushOutdated owner;

    public TaskStartRush(aa_RushOutdated owner) 
    {
        this.owner = owner;
    }

    public override Status Check(float dt)
    {
        // Set up the enemy for the rush
        owner.isRushing = true;

        status = Status.SUCCESS;
        return status;
    }
}
