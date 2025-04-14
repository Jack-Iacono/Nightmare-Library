using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    public static bool gameStarted = false;

    public const float gameTime = 360;
    public float gameTimer { get; set; } = gameTime;

    private const int totalLevels = 5;
    private int gameTimeLevel = 1;
    public delegate void OnLevelChangeDelegate(int theshold);
    public static OnLevelChangeDelegate OnLevelChange;

    public static int enemyCount = 1;
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    public static RoundResults roundResults = null;

    public static bool isNetworkGame = true;

    public delegate void OnGameEndDelegate();
    public static event OnGameEndDelegate OnGameEnd;

    public delegate void OnGameStartDelegate();
    public static event OnGameStartDelegate OnGameStart;

    public delegate void OnEnemyCountChangedDelegate(int count);
    public static event OnEnemyCountChangedDelegate OnEnemyCountChanged;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);

        roundResults = new RoundResults(enemyCount);
        SceneController.OnMapLoaded += OnMapLoaded;

        PlayerController.OnPlayerAliveChanged += OnPlayerAliveChanged;
    }

    private void OnMapLoaded(string mapName)
    {
        if (NetworkConnectionController.HasAuthority && SceneController.loadedMap.name == SceneController.scenes[SceneController.m_Scene.GAME].name)
        {
            for (int i = 0; i < GameController.enemyCount; i++)
            {
                GameObject ePrefab = PrefabHandler.Instance.InstantiatePrefab(PrefabHandler.Instance.e_Enemy, new Vector3(-20, 1, 0), Quaternion.identity);
                ePrefab.name = "Basic Enemy";
                spawnedEnemies.Add(ePrefab);
            }
        }

        gameStarted = true;
        OnGameStart?.Invoke();
    }

    // Update is called once per frame
    void Update()
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

    public static void SetEnemyCount(int e)
    {
        enemyCount = e;
        OnEnemyCountChanged?.Invoke(enemyCount);
    }

    public void OnPlayerAliveChanged(PlayerController player, bool b)
    {
        bool allPlayersDead = true;
        foreach (PlayerController p in PlayerController.playerInstances.Values)
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

        ((GameLobbyController)LobbyController.instance).GoToPreGame();
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;

        foreach(GameObject g in spawnedEnemies)
        {
            Destroy(g);
        }

        foreach (PlayerController p in PlayerController.playerInstances.Values)
        {
            p.ChangeAliveState(true);
        }

        gameStarted = false;
        SceneController.OnMapLoaded -= OnMapLoaded;
        PlayerController.OnPlayerAliveChanged -= OnPlayerAliveChanged;
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
        public void SetPresentEnemies(Dictionary<GameObject, Enemy> enemies)
        {
            List<Enemy> e = new List<Enemy>(enemies.Values);
            for(int i = 0; i < e.Count; i++)
            {
                if (presentEnemies.Count > i)
                    presentEnemies[i] = e[i].enemyType;
                else
                    presentEnemies.Add(e[i].enemyType);
            }
        }
    }
}
