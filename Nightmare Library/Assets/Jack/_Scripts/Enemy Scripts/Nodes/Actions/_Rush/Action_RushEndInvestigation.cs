using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class Action_RushEndInvestigation : Node
{
    private aa_Rush owner;

    public Action_RushEndInvestigation(aa_Rush owner)
    {
        this.owner = owner;
    }

    public override Status Check(float dt)
    {
        owner.RemoveAudioSource(0);
        return Status.SUCCESS;
    }
}
