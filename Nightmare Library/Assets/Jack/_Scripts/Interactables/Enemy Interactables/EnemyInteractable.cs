using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInteractable : Interactable
{
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void EnemyInteractHysterics(bool fromNetwork = false)
    {
        rb.AddForce(new Vector3(1,1,1) * 10, ForceMode.Impulse);

        base.EnemyInteractHysterics();
    }
}
