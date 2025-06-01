using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Check_ConditionHasStaticTarget : Check_Condition
{
    private ActiveAttack owner;
    public Check_ConditionHasStaticTarget(ActiveAttack owner) : base()
    {
        this.owner = owner;
    }
    protected override bool EvaluateCondition()
    {
        return owner.currentTargetStatic != Vector3.zero;
    }
}
