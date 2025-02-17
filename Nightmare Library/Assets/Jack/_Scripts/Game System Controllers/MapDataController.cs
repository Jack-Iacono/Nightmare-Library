using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDataController : MonoBehaviour
{
    public static MapDataController Instance { get; private set; }

    public Vector3 playerSpawnPoint;


    public delegate void OnSpawnPointRegisterDelegate();
    public static OnSpawnPointRegisterDelegate OnSpawnPointRegister;

    private void Awake()
    {
        if(Instance != null)
            Destroy(Instance);

        Instance = this;
        OnSpawnPointRegister?.Invoke();
    }
    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
