using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class CheckRushInvestigationEnd : Node
{
    private aa_Rush owner;

    public CheckRushInvestigationEnd(aa_Rush owner) : base()
    {
        this.owner = owner;
    }

    public override Status Check(float dt)
    {
        if (owner.nodeQueue.Count > 1)
        {
            owner.RefreshPath();
            return Status.RUNNING;
        }
        else
        {
            return Status.SUCCESS;
        }
    }
}
