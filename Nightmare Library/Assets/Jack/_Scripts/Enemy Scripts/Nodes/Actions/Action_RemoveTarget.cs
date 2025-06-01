using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class Action_RemoveTarget : Node
{
    ActiveAttack owner;
    public Action_RemoveTarget(ActiveAttack owner) : base()
    {
        this.owner = owner;
    }
    public override Status Check(float dt)
    {
        owner.SetCurrentTarget(null);

        status = Status.SUCCESS;
        return status;
    }
}
