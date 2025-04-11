using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootprintController : HoldableItem
{
    private float avgDuration = 10;
    private float dev = 1;

    private bool isRunning;
    private float currentLifeTimer;

    private void Start()
    {
        if(!NetworkConnectionController.connectedToLobby)
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
                Pickup();
            }
        }
    }

    public override void Place(Vector3 pos, Quaternion rot, bool fromNetwork = false)
    {
        currentLifeTimer = UnityEngine.Random.Range(avgDuration - dev, avgDuration + dev); 
        isRunning = true;

        base.Place(pos, rot, fromNetwork);
    }
    public override GameObject Pickup(bool fromNetwork = false)
    {
        isRunning = false;
        gameObject.SetActive(false);

        return base.Pickup(fromNetwork);
    }
}
