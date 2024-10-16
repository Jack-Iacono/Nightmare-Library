using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckStalkTargetInMap : Node
{
    private aa_Stalk owner;
    public CheckStalkTargetInMap(aa_Stalk owner)
    {
        this.owner = owner;
    }
    public override Status Check(float dt)
    {
        if (DeskController.playersAtDesk.Contains(owner.currentTargetPlayer))
        {
            status = Status.SUCCESS;
            return status;
        }
        status = Status.FAILURE;
        return status;
    }
}
