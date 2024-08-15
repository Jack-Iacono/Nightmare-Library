using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootprintController : MonoBehaviour
{
    private float avgDuration = 10;
    private float dev = 1;

    private bool isRunning;
    private float currentLifeTimer;

    public event EventHandler OnFootprintSpawn;
    public event EventHandler OnFootprintDespawn;

    private void Start()
    {
        if(!NetworkConnectionController.IsRunning)
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

    public void Activate()
    {
        currentLifeTimer = UnityEngine.Random.Range(avgDuration - dev, avgDuration + dev);  isRunning = true;

        OnFootprintSpawn?.Invoke(this, EventArgs.Empty);
    }
    public void Deactivate()
    {
        isRunning = false;
        gameObject.SetActive(false);

        OnFootprintDespawn?.Invoke(this, EventArgs.Empty);
    }
}
