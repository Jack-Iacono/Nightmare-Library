using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapController : HoldableItem
{
    private float trapDuration = 5;

    private float avgDuration = 100;
    private float dev = 5;

    private bool isRunning;
    private float currentLifeTimer;

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
                Pickup();
            }
        }
    }

    public override void Place(Vector3 pos, Quaternion rot, bool fromNetwork = false)
    {
        currentLifeTimer = UnityEngine.Random.Range(avgDuration - dev, avgDuration + dev); isRunning = true;
        gameObject.SetActive(true);
        isRunning = true;
        base.Place(pos, rot, fromNetwork);
    }
    public override GameObject Pickup(bool fromNetwork = false)
    {
        isRunning = false;
        gameObject.SetActive(false);
        return base.Pickup(fromNetwork);
    }

    // OPTIMIZE
    private void OnTriggerEnter(Collider other)
    {
        if(PlayerController.playerInstances.ContainsKey(other.gameObject))
        {
            PlayerController p = PlayerController.playerInstances[other.gameObject];

            if(p == PlayerController.mainPlayerInstance)
            {
                other.GetComponent<PlayerController>().Trap(trapDuration);
                Pickup();
            }
        }
    }
}
