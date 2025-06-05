using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node_CheckStalkTargetOutOffice : Node_CheckCondition
{
    private aa_Stalk owner;
    public Node_CheckStalkTargetOutOffice(aa_Stalk owner)
    {
        this.owner = owner;
    }

    protected override bool EvaluateCondition()
    {
        return DeskController.playersAtDesk.Contains(owner.currentTargetPlayer);
    }
}
