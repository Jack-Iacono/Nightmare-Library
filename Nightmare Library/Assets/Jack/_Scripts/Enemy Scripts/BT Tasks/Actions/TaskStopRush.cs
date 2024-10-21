using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;
using UnityEngine.AI;

public class TaskStopRush : Node
{
    private aa_RushOutdated owner;

    public TaskStopRush(aa_RushOutdated owner)
    {
        this.owner = owner;
    }

    public override Status Check(float dt)
    {
        owner.isRushing = false;

        status = Status.SUCCESS;
        return status;
    }
}
