using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DeskController : MonoBehaviour
{
    public static DeskController instance;

    public static List<PlayerController> playersAtDesk = new List<PlayerController>();

    private void Awake()
    {
        if (instance != null)
            Destroy(instance);

        instance = this;
        PlayerController.OnPlayerKilled += OnPlayerKilled;
    }

    // Start is called before the first frame update
    void Start()
    {
        playersAtDesk = new List<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            if (PlayerController.playerInstances[other.gameObject].isAlive)
                playersAtDesk.Add(PlayerController.playerInstances[other.gameObject]);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            if (PlayerController.playerInstances[other.gameObject].isAlive)
                playersAtDesk.Remove(PlayerController.playerInstances[other.gameObject]);
        }
    }

    private void OnPlayerKilled(object sender, EventArgs e)
    {
        if(playersAtDesk.Contains((PlayerController)sender))
            playersAtDesk.Remove(PlayerController.playerInstances[((PlayerController)sender).gameObject]);
    }

    private void OnDestroy()
    {
        if(instance == this)
            instance = null;

        PlayerController.OnPlayerKilled -= OnPlayerKilled;
    }
}
