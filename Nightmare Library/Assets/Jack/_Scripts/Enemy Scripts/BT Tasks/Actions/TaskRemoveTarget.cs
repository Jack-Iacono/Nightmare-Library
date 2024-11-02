using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class TaskRemoveTarget : Node
{
    ActiveAttack owner;
    public TaskRemoveTarget(ActiveAttack owner) : base()
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
