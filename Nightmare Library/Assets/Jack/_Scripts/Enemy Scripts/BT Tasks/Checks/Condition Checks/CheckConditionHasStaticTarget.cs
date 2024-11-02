using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckConditionHasStaticTarget : CheckCondition
{
    private ActiveAttack owner;
    public CheckConditionHasStaticTarget(ActiveAttack owner) : base()
    {
        this.owner = owner;
    }
    protected override bool EvaluateCondition()
    {
        return owner.currentTargetStatic != Vector3.zero;
    }
}
