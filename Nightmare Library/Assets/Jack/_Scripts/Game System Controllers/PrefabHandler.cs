using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PrefabHandler : MonoBehaviour
{
    public static PrefabHandler Instance;
    private PrefabHandlerNetwork network = null;

    [Header("Player Prefabs")]
    public GameObject p_Player;

    [Header("Enemy Prefabs")]
    public GameObject e_Enemy;
    public GameObject e_EvidenceFootprint;
    public GameObject e_EvidenceTrap;
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
        GameObject g = Instantiate(obj, pos, rot);

        if (network != null)
            network.InstantiatePrefab(g);
        else
        {
            // Removes network behavior from the object since it is not needed
            NetworkBehaviour b;
            if(g.TryGetComponent(out b))
            {
                Destroy(b);
                Destroy(g.GetComponent<NetworkObject>());
            }
        }

        return g;
    }
    public GameObject InstantiatePrefabOnline(GameObject obj, Vector3 pos, Quaternion rot, ulong owner = ulong.MaxValue)
    {
        GameObject g = Instantiate(obj, pos, rot);
        network.InstantiatePrefab(g, owner);
        return g;
    }
    public GameObject InstantiatePrefabOffline(GameObject obj, Vector3 pos, Quaternion rot)
    {
        return Instantiate(obj, pos, rot);
    }

    public void SetNetwork(PrefabHandlerNetwork network)
    {
        this.network = network;
    }
}
