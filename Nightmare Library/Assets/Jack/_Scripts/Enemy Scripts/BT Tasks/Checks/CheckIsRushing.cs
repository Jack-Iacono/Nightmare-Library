using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class CheckIsRushing : Node
{
    private aa_Rush owner;

    public CheckIsRushing(aa_Rush owner) : base()
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
