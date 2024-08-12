using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInteractable : Interactable
{
    private Rigidbody rb;

    public event EventHandler OnInteract;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void EnemyInteract()
    {
        rb.AddForce(new Vector3(1,1,1) * 10, ForceMode.Impulse);

        OnInteract?.Invoke(this, EventArgs.Empty);
    }
}
