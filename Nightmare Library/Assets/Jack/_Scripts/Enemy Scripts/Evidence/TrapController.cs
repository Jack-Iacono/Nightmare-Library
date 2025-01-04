using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapController : MonoBehaviour
{
    private float trapDuration = 5;

    private float avgDuration = 100;
    private float dev = 5;

    private bool isRunning;
    private float currentLifeTimer;

    public event EventHandler<bool> OnTrapSpawn;
    public event EventHandler<bool> OnTrapDespawn;

    private void Start()
    {
        if (!NetworkConnectionController.IsRunning)
            gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (isRunning)
        {
            if (currentLifeTimer > 0)
            {
                currentLifeTimer -= Time.deltaTime;
            }
            else
            {
                Deactivate();
            }
        }
    }

    public void Activate(bool fromNetwork = false)
    {
        currentLifeTimer = UnityEngine.Random.Range(avgDuration - dev, avgDuration + dev); isRunning = true;

        OnTrapSpawn?.Invoke(this, fromNetwork);
    }
    public void Deactivate(bool fromNetwork = false)
    {
        isRunning = false;
        gameObject.SetActive(false);

        OnTrapDespawn?.Invoke(this, fromNetwork);
    }

    // OPTIMIZE
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            PlayerController p = other.GetComponent<PlayerController>();

            if(p != null && p == PlayerController.ownerInstance)
            {
                other.GetComponent<PlayerController>().Trap(trapDuration);
                Deactivate();
            }
        }
    }
}
