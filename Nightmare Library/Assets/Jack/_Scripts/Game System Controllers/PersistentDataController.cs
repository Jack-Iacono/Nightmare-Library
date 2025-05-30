using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentDataController : MonoBehaviour
{
    public static PersistentDataController Instance;

    public bool loadTestScene = false;
    public List<EnemyPreset> activeEnemyPresets = new List<EnemyPreset>();

    private void Awake()
    {
        if(Instance != null)
            Destroy(Instance);

        Instance = this;
    }

    private void OnDestroy()
    {
        if(Instance == this) 
            Instance = null;
    }
}
