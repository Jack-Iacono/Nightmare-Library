using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Check_ConditionWardenAlert : Check_Condition
{
    private aa_Warden owner;

    public Check_ConditionWardenAlert(aa_Warden owner) : base()
    {
        this.owner = owner;
    }
    protected override bool EvaluateCondition()
    {
        return !owner.IsAlertEmpty();
    }
}
