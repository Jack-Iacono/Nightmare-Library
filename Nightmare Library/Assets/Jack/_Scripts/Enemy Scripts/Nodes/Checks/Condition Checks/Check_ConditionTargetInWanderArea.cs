using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Check_ConditionTargetInWanderArea : Check_Condition
{
    aa_Warden owner;
    public Check_ConditionTargetInWanderArea(aa_Warden owner) : base()
    {
        this.owner = owner;
    }
    protected override bool EvaluateCondition()
    {
        // POTENTIAL DISTANCE OPTIMIZATION
        return owner.currentTargetStatic != Vector3.zero && Vector3.Distance(owner.currentTargetStatic, owner.areaCenter.position) > owner.wanderRange;
    }
}
