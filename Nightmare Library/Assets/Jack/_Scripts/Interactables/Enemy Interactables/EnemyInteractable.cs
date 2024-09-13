using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInteractable : Interactable
{
    public override void EnemyInteractHysterics(bool fromNetwork = false)
    {
        rb.AddForce
            (
            new Vector3
                (UnityEngine.Random.Range(0, 10),
                UnityEngine.Random.Range(4, 10),
                UnityEngine.Random.Range(0, 10)
                ) * 10, 
            ForceMode.Impulse
            );

        base.EnemyInteractHysterics();
    }
}
