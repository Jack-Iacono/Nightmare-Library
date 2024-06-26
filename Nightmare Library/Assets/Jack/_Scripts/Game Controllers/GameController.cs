using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    public static bool gamePaused = false;

    public GameObject offlinePlayerPrefab;
    public GameObject offlineEnemyPrefab;

    public float timer = 0;
    public Transform[] patrolPoints;

    // Local Events
    public static event EventHandler<bool> OnGamePause;

    public static bool isNetworkGame = true;

    // Multiplayer Events
    public static event EventHandler<bool> OnNetworkGamePause;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }
    private void Start()
    {
        if (!NetworkManager.Singleton.IsServer)
            SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        Instantiate(offlinePlayerPrefab);
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
