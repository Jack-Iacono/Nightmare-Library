using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Check_ConditionStalkTargetOutOffice : Check_Condition
{
    private aa_Stalk owner;
    public Check_ConditionStalkTargetOutOffice(aa_Stalk owner)
    {
        this.owner = owner;
    }

    protected override bool EvaluateCondition()
    {
        return DeskController.playersAtDesk.Contains(owner.currentTargetPlayer);
    }
}
