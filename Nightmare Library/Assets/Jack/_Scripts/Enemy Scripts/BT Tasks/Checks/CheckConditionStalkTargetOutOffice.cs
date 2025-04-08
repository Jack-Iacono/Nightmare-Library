using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckConditionStalkTargetOutOffice : CheckCondition
{
    private aa_Stalk owner;
    public CheckConditionStalkTargetOutOffice(aa_Stalk owner)
    {
        this.owner = owner;
    }

    protected override bool EvaluateCondition()
    {
        return DeskController.playersAtDesk.Contains(owner.currentTargetPlayer);
    }
}
