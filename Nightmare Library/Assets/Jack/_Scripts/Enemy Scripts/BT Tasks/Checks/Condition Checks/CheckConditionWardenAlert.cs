using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckConditionWardenAlert : CheckCondition
{
    private aa_Warden owner;

    public CheckConditionWardenAlert(aa_Warden owner) : base()
    {
        this.owner = owner;
    }
    protected override bool EvaluateCondition()
    {
        return !owner.IsAlertEmpty();
    }
}
