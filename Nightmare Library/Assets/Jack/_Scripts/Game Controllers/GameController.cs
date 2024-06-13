using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    public static bool gamePaused = false;

    public float timer = 0;

    // Local Events
    public static event EventHandler<bool> OnGamePause;

    private bool isNetworkGame = true;

    // Multiplayer Events
    public static event EventHandler<bool> OnNetworkGamePause;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            PauseGame(!gamePaused);
            
            if(isNetworkGame)
                OnNetworkGamePause?.Invoke(this, gamePaused);
        }
    }

    public void PauseGame(bool b)
    {
        gamePaused = b;
        OnGamePause?.Invoke(this, b);
    }


}
