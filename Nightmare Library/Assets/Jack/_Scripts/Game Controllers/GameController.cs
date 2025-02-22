using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    public static bool gamePaused = false;

    public const float gameTime = 50;
    public float gameTimer { get; set; } = gameTime;

    private const int totalLevels = 5;
    private int gameTimeLevel = 1;
    public delegate void OnLevelChangeDelegate(int theshold);
    public static OnLevelChangeDelegate OnLevelChange;

    public List<EnemyPreset> enemyPresets = new List<EnemyPreset>();
    public const int enemyCount = 1;
    private static List<EnemyPreset> enemyGuesses = new List<EnemyPreset>(enemyCount);

    // Local Events
    public static event EventHandler<bool> OnGamePause;

    public static bool isNetworkGame = true;

    // Multiplayer Events
    public static event EventHandler<bool> OnNetworkGamePause;

    public delegate void OnGameEndDelegate();
    public static event OnGameEndDelegate OnGameEnd;

    public delegate void OnReturnToMenuDelegate();
    public static event OnReturnToMenuDelegate OnReturnToMenu;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);

        PlayerController.OnPlayerKilled += OnPlayerKilled;
        for (int i = 0; i < enemyCount; i++)
        {
            enemyGuesses.Add(null);
        }
    }
    private void Start()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            SpawnPrefabs();
        PauseGame(false);
    }

    private void SpawnPrefabs()
    {
        PrefabHandler.Instance.InstantiatePrefab(PrefabHandler.Instance.p_Player, new Vector3(-20, 1, 0), Quaternion.identity);
        for(int i = 0; i < enemyCount; i++)
        {
            PrefabHandler.Instance.InstantiatePrefab(PrefabHandler.Instance.e_Enemy, new Vector3(-20, 1, 0), Quaternion.identity);
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (!gamePaused)
        {
            if (gameTimer > 0)
            {
                gameTimer -= Time.deltaTime;

                // Tells the other scripts that the game level is increasing, this makes the game more difficult
                if (gameTime - gameTimer > gameTimeLevel * (gameTime / totalLevels))
                {
                    gameTimeLevel++;
                    OnLevelChange?.Invoke(gameTimeLevel);
                }
            }
            else
                EndGame();
        }
    }

    private void OnPlayerKilled(PlayerController player)
    {
        bool allPlayersDead = true;
        foreach(PlayerController p in PlayerController.playerInstances.Values)
        {
            if (p.isAlive)
            {
                allPlayersDead = false;
                break;
            }
        }

        if (allPlayersDead && NetworkConnectionController.HasAuthority)
        {
            EndGame();
        }
    }

    public static void MakeGuess(int index, EnemyPreset preset)
    {
        
        enemyGuesses[index] = preset;
    }

    public void EndGame()
    {
        if(NetworkConnectionController.HasAuthority)
        {
            PauseGame(true);
            OnGameEnd?.Invoke();
        }

        for(int i = 0; i < enemyCount; i++)
        {
            if(enemyGuesses[i] != null && Enemy.enemyInstances[i].enemyType == enemyGuesses[i])
                Debug.Log("Guess " + i + " is correct");
            else
                Debug.Log("Guess " + i + " is wrong");
        }

        // Load the end screen
        UIController.mainInstance.ChangeToScreen(1);
    }
    public static void ReturnToMenu()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            OnReturnToMenu?.Invoke();
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
