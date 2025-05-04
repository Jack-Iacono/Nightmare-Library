using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDataController : MonoBehaviour
{
    public static MapDataController Instance { get; private set; }

    public Vector3 playerSpawnPoint;
    public List<Vector3> enemySpawnPoints;

    public delegate void OnMapDataLoadedDelegate();
    public static OnMapDataLoadedDelegate OnMapDataLoaded;

    private void Awake()
    {
        if (Instance != null)
            Destroy(Instance);

        Instance = this;

        OnMapDataLoaded?.Invoke();
    }

    public static Vector3 GetRandomEnemySpawnPoint()
    {
        return Instance.enemySpawnPoints[Random.Range(0, Instance.enemySpawnPoints.Count)];
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
