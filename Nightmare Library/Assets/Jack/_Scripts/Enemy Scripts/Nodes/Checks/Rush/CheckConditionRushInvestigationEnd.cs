using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckConditionRushInvestigationEnd : Check_Condition
{
    private aa_Rush owner;

    public CheckConditionRushInvestigationEnd(aa_Rush owner)
    {
        this.owner = owner;
    }   

    protected override bool EvaluateCondition()
    {
        return owner.nodeQueue.Count <= 1;
    }
}
