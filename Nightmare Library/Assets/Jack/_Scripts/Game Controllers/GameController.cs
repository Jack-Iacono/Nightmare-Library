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

    public delegate void OnPlayerKilledDelegate(PlayerController player);
    public static event OnPlayerKilledDelegate OnPlayerKilled;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);

        for (int i = 0; i < enemyCount; i++)
        {
            enemyGuesses.Add(null);
        }
    }
    private void Start()
    {
        PauseGame(false);
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

    public static void NotifyPlayerKilled(PlayerController player)
    {
        OnPlayerKilled?.Invoke(player);

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

    public static void EndGame()
    {
        if(NetworkConnectionController.HasAuthority)
        {
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
        ((GameLobbyController)LobbyController.instance).ReturnToPreGame();
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
    }

}
