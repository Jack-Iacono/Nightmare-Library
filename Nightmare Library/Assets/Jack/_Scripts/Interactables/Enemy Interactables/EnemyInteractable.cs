using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInteractable : Interactable
{
    public override void EnemyInteractHysterics(bool fromNetwork = false)
    {
        rb.AddForce(new Vector3(1, 1, 1) * 10, ForceMode.Impulse);

        base.EnemyInteractHysterics();
    }
}
