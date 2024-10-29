using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPrefabHandler : MonoBehaviour
{
    public static EnemyPrefabHandler Instance;

    [Header("Offline Prefabs")]
    public GameObject wardenSensor;

    private void Awake()
    {
        if(Instance != null)
            Destroy(Instance);
        Instance = this;
    }
}
