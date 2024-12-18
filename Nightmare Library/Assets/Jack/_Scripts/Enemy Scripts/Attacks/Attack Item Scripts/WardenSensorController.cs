using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WardenSensorController : Interactable
{
    public delegate void OnSensorAlert(WardenSensorController controller);
    public event OnSensorAlert onSensorAlert;

    private void OnTriggerEnter(Collider other)
    {
        // Check if a player is the one triggering the sensor
        foreach(PlayerController p in PlayerController.playerInstances)
        {
            if(p.gameObject == other.gameObject)
            {
                NotifySensorTriggered();
                break;
            }
        }
    }

    private void NotifySensorTriggered()
    {
        onSensorAlert?.Invoke(this);
    }
}
