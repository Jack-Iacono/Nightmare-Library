using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node_CheckRushInvestigationEnd : Node_CheckCondition
{
    private aa_Rush owner;

    public Node_CheckRushInvestigationEnd(aa_Rush owner)
    {
        this.owner = owner;
    }   

    protected override bool EvaluateCondition()
    {
        return owner.nodeQueue.Count <= 1;
    }
}
