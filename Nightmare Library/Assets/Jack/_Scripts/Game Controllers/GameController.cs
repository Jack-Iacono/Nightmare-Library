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

    // For testing purposes
    [SerializeField]
    private bool spawnEnemies = true;

    public const float gameTime = 360;
    public float gameTimer { get; set; } = gameTime;

    private const int totalLevels = 5;
    private int gameTimeLevel = 1;
    public delegate void OnLevelChangeDelegate(int theshold);
    public static OnLevelChangeDelegate OnLevelChange;

    public static int startingEnemyCount = 1;
    public static int currentEnemyCount = 0;
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    public static GameInfo gameInfo = null;

    public static bool isNetworkGame = true;

    public delegate void OnGameEndDelegate(int endReason);
    public static event OnGameEndDelegate OnGameEnd;

    public delegate void OnGameStartDelegate();
    public static event OnGameStartDelegate OnGameStart;

    public delegate void OnEnemyCountChangedDelegate(int count);
    public static event OnEnemyCountChangedDelegate OnEnemyCountChanged;

    public delegate void OnGameInfoChangedDelegate();
    public static event OnGameInfoChangedDelegate OnGameInfoChanged;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);

        // Ensure to overwrite the previous data
        gameInfo = new GameInfo();
        SceneController.OnMapLoaded += OnMapLoaded;

        PlayerController.OnPlayerAliveChanged += OnPlayerAliveChanged;
        Enemy.OnEnemyRemoved += OnEnemyRemoved;
    }

    private void OnMapLoaded(string mapName)
    {
        if (NetworkConnectionController.HasAuthority && SceneController.loadedMap.name == SceneController.scenes[SceneController.m_Scene.GAME].name)
        {
            if (spawnEnemies)
            {
                for (int i = 0; i < GameController.startingEnemyCount; i++)
                {
                    Vector3 pos = MapDataController.GetRandomEnemySpawnPoint();
                    GameObject ePrefab = PrefabHandler.Instance.InstantiatePrefab(PrefabHandler.Instance.e_Enemy, pos, Quaternion.identity);
                    ePrefab.name = "Basic Enemy";
                    spawnedEnemies.Add(ePrefab);

                    currentEnemyCount++;
                }
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
        {
            EndGame(0);
        }
    }

    public static void SetEnemyCount(int e)
    {
        startingEnemyCount = e;
        OnEnemyCountChanged?.Invoke(startingEnemyCount);
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
            StartCoroutine(BeginEndGame(2));
        }
    }
    private void OnEnemyRemoved(Enemy enemy)
    {
        currentEnemyCount--;

        // TEMPORARY!!!!
        // Will be removed after demo
        if(currentEnemyCount <= 0)
        {
            StartCoroutine(BeginEndGame(1));
        }
    }

    private IEnumerator BeginEndGame(int endReason)
    {
        yield return new WaitForSecondsRealtime(5);
        EndGame(endReason);
    }
    public static void EndGame(int endReason)
    {
        // End reason -> 0: Timer, 1: All Enemies Dead, 2: All Players Dead

        if(NetworkConnectionController.HasAuthority)
        {
            OnGameEnd?.Invoke(endReason);
        }

        // Tells the round results how the game ended
        gameInfo.SetEndReason(endReason);

        // Send the players to the pre game lobby
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

    public class GameInfo
    {
        public int endReason { get; private set; } = -1;
        public List<EnemyPreset> presentEnemies = new List<EnemyPreset>(startingEnemyCount);

        public void SetEndReason(int endReason)
        {
            this.endReason = endReason;
            OnGameInfoChanged?.Invoke();
        }
        public void AddPresentEnemy(EnemyPreset enemy)
        {
            presentEnemies.Add(enemy);
            OnGameInfoChanged?.Invoke();
        }
    }
}
