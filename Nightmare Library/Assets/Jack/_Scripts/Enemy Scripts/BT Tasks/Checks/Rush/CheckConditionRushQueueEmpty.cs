using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckConditionRushQueueEmpty : CheckCondition
{
    private aa_Rush owner;

    public CheckConditionRushQueueEmpty(aa_Rush owner)
    {
        this.owner = owner;
    }   

    protected override bool EvaluateCondition()
    {
        return owner.nodeQueue.Count <= 0;
    }
}
