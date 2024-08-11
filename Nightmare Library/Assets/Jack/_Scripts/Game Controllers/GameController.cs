using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    public static bool gamePaused = false;

    public GameObject offlinePlayerPrefab;
    public GameObject offlineEnemyPrefab;

    public Transform[] patrolPoints;

    public float gameTimer = 120;

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
        PauseGame(false);
    }

    private void SpawnPrefabs()
    {
        Instantiate(offlinePlayerPrefab, new Vector3(-20,1,0), Quaternion.identity);
        Instantiate(offlineEnemyPrefab, new Vector3(-20, 1, 0), Quaternion.identity);
    }


    // Update is called once per frame
    void Update()
    {
        if (!gamePaused)
        {
            if (gameTimer > 0)
                gameTimer -= Time.deltaTime;
            else
                EndGame();

            if (Input.GetKeyDown(KeyCode.Q))
            {
                PauseGame(!gamePaused);
            }
        }
    }

    private void OnPlayerKilled(object sender, EventArgs e)
    {
        bool allPlayersDead = true;
        foreach(PlayerController p in PlayerController.playerInstances)
        {
            if (p.isAlive)
            {
                allPlayersDead = false;
                break;
            }
        }

        if (allPlayersDead && NetworkConnectionController.HasAuthority)
        {
            PauseGame(true);
            StartCoroutine(EndGameCoroutine());
        }
    }

    public IEnumerator EndGameCoroutine()
    {
        yield return new WaitForSeconds(5);
        EndGame();
    }
    public void EndGame()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            OnGameEnd?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            SceneController.LoadScene(SceneController.m_Scene.MAIN_MENU);
        }
    }

    public void PauseGame(bool b)
    {
        if (isNetworkGame && NetworkConnectionController.HasAuthority)
            OnNetworkGamePause?.Invoke(this, b);

        gamePaused = b;
        OnGamePause?.Invoke(this, b);
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;

        PlayerController.OnPlayerKilled -= OnPlayerKilled;
    }

}
