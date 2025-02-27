using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckConditionAudioSourcePresent : CheckCondition
{
    private Attack owner;

    public CheckConditionAudioSourcePresent(Attack owner) : base()
    {
        this.owner = owner;
    }

    protected override bool EvaluateCondition()
    {
        return owner.GetAudioSource(0) != null;
    }
}
