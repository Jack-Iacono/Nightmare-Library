using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_WardenRemoveAlert : Node
{
    aa_Warden owner;

    public Action_WardenRemoveAlert(aa_Warden owner) : base()
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
