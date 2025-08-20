using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node_CheckAudioSourcePresent : Node_CheckCondition
{
    private Attack owner;

    public Node_CheckAudioSourcePresent(Attack owner) : base()
    {
        this.owner = owner;
    }

    protected override bool EvaluateCondition()
    {
        return owner.GetAudioSource(0) != null;
    }
}
