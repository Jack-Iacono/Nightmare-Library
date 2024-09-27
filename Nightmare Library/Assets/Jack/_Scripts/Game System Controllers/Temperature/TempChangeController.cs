using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempChangeController : Interactable
{
    public override void Click(bool fromNetwork = false)
    {
        TempController.ChangeTempState();
        base.Click(fromNetwork);
    }
}
