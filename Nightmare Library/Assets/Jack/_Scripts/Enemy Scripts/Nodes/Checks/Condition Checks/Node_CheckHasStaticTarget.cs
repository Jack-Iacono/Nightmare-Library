using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node_CheckHasStaticTarget : Node_CheckCondition
{
    private ActiveAttack owner;
    public Node_CheckHasStaticTarget(ActiveAttack owner) : base()
    {
        this.owner = owner;
    }
    protected override bool EvaluateCondition()
    {
        return owner.currentTargetStatic != Vector3.zero;
    }
}
