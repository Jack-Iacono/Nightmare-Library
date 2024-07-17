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
    public static event EventHandler OnGameEnd;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);

        PlayerController.OnPlayerKilled += OnPlayerKilled;
    }

    private void Start()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            SpawnPrefabs();
    }

    private void SpawnPrefabs()
    {
        Instantiate(offlinePlayerPrefab, new Vector3(-20,1,0), Quaternion.identity);
        Instantiate(offlineEnemyPrefab, new Vector3(-20, 1, 0), Quaternion.identity);
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

    private void OnPlayerKilled(object sender, EventArgs e)
    {
        bool gameEnd = true;
        foreach(PlayerController p in PlayerController.playerInstances)
        {
            if (p.isAlive)
            {
                gameEnd = false;
                break;
            }
        }

        if (gameEnd)
        {
            EndGame();
        }
    }

    private void EndGame()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            OnGameEnd?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            OfflineSceneController.ChangeScene(OfflineSceneController.m_Scene.MAIN_MENU);
        }
    }

    public void PauseGame(bool b)
    {
        gamePaused = b;
        OnGamePause?.Invoke(this, b);
    }

}
