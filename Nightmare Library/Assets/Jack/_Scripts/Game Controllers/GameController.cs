using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    public static bool gamePaused = false;

    public const float gameTime = 20;
    public float gameTimer { get; set; } = gameTime;

    private const int totalLevels = 5;
    private int gameTimeLevel = 1;
    public delegate void OnLevelChangeDelegate(int theshold);
    public static OnLevelChangeDelegate OnLevelChange;

    public List<EnemyPreset> enemyPresets = new List<EnemyPreset>();
    public const int enemyCount = 1;

    public static RoundResults roundResults;

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

        roundResults = new RoundResults(enemyCount);
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
        roundResults.SetGuess(index, preset);
    }

    public static void EndGame()
    {
        if(NetworkConnectionController.HasAuthority)
        {
            OnGameEnd?.Invoke();
        }

        roundResults.SetPresentEnemies(Enemy.enemyInstances);

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

    public class RoundResults
    {
        public List<EnemyPreset> enemyGuesses = new List<EnemyPreset>(enemyCount);
        public List<EnemyPreset> presentEnemies = new List<EnemyPreset>(enemyCount);

        public RoundResults(int enemyCount)
        {
            for (int i = 0; i < enemyCount; i++)
            {
                enemyGuesses.Add(null);
                presentEnemies.Add(null);
            }
        }

        public void SetGuess(int i, EnemyPreset e)
        {
            enemyGuesses[i] = e;    
        }
        public void SetPresentEnemies(List<Enemy> list)
        {
            for(int i = 0; i < list.Count; i++)
            {
                if (presentEnemies.Count > i)
                    presentEnemies[i] = list[i].enemyType;
                else
                    presentEnemies.Add(list[i].enemyType);
            }
        }
    }
}
