using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class CheckIsRushing : Node
{
    private aa_RushOutdated owner;

    public CheckIsRushing(aa_RushOutdated owner) : base()
    {
        this.owner = owner;
    }
    public override Status Check(float dt)
    {
        if (owner.isRushing)
        {
            status = Status.SUCCESS;
            return status;
        }

        status = Status.FAILURE;
        return status;
    }
}
