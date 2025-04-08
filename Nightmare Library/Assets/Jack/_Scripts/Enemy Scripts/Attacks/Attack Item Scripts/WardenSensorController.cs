using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WardenSensorController : HoldableItem
{
    public delegate void OnSensorAlert(WardenSensorController controller);
    public event OnSensorAlert onSensorAlert;

    private AudioSourceController audioController;

    protected override void Awake()
    {
        audioController = GetComponent<AudioSourceController>();
        base.Awake();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if a player is the one triggering the sensor
        foreach (PlayerController p in PlayerController.playerInstances.Values)
        {
            if (p.gameObject == other.gameObject)
            {
                NotifySensorTriggered();
                audioController.Play(AudioManager.GetAudioData(AudioManager.SoundType.e_WARDEN_SENSOR_STEP));
                break;
            }
        }
    }

    private void NotifySensorTriggered()
    {
        onSensorAlert?.Invoke(this);
    }
}
