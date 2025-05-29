using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class Action_WardenClearAlert : Node
{
    aa_Warden owner;
    public Action_WardenClearAlert(aa_Warden owner) : base()
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
