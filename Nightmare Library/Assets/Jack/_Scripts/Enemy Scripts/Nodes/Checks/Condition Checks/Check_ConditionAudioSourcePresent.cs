using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Check_ConditionAudioSourcePresent : Check_Condition
{
    private Attack owner;

    public Check_ConditionAudioSourcePresent(Attack owner) : base()
    {
        this.owner = owner;
    }

    protected override bool EvaluateCondition()
    {
        return owner.GetAudioSource(0) != null;
    }
}
