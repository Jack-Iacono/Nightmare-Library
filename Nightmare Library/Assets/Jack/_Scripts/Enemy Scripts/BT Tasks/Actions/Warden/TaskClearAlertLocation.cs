using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskClearAlertLocation : Node
{
    aa_Warden owner;

    public TaskClearAlertLocation(aa_Warden owner) : base()
    {
        this.owner = owner;
    }
    public override Status Check(float dt)
    {
        owner.RemoveAlertItem();

        status = Status.SUCCESS;
        return status;
    }
}
