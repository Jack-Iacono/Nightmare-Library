using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabHandler : MonoBehaviour
{
    public static PrefabHandler Instance;
    private PrefabHandlerNetwork network = null;

    [Header("Enemy Prefabs")]
    public GameObject e_WardenSensor;
    public GameObject e_ScreechHead;

    private void Awake()
    {
        if(Instance != null)
            Destroy(Instance);
        Instance = this;
    }

    public GameObject InstantiatePrefab(GameObject obj, Vector3 pos, Quaternion rot)
    {
        if(network != null)
            return network.InstantiatePrefab(obj, pos, rot);
        else
            return Instantiate(obj, pos, rot);
    }

    public void SetNetwork(PrefabHandlerNetwork network)
    {
        this.network = network;
    }
}
