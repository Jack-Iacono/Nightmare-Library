using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class TaskWardenClearAlertQueue : Node
{
    aa_Warden owner;
    public TaskWardenClearAlertQueue(aa_Warden owner) : base()
    {
        this.owner = owner;
    }
    public override Status Check(float dt)
    {
        owner.ClearAlertItems();

        Debug.Log("Clearing Alert Queue");

        status = Status.SUCCESS;
        return status;
    }
}
