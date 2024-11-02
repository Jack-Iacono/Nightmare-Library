using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckConditionTargetInWanderArea : CheckCondition
{
    aa_Warden owner;
    public CheckConditionTargetInWanderArea(aa_Warden owner) : base()
    {
        this.owner = owner;
    }
    protected override bool EvaluateCondition()
    {
        // POTENTIAL DISTANCE OPTIMIZATION
        return owner.currentTargetStatic != Vector3.zero && Vector3.Distance(owner.currentTargetStatic, owner.areaCenter.position) > owner.wanderRange;
    }
}
