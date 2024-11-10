using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(PrefabHandler))]
public class PrefabHandlerNetwork : NetworkBehaviour
{
    public static PrefabHandlerNetwork Instance;
    private PrefabHandler parent;

    private List<NetworkObject> spawnedObjects = new List<NetworkObject>();

    public Dictionary<GameObject, GameObject> prefabLink = new Dictionary<GameObject, GameObject>();

    [Header("Player Prefabs")]
    public GameObject p_PlayerOnline;

    [Header("Enemy Prefabs")]
    public GameObject e_EnemyOnline;
    public GameObject e_WardenSensorOnline;
    public GameObject e_ScreechHeadOnline;

    private void Awake()
    {
        if (Instance != null)
            Destroy(Instance);
        Instance = this;

        parent = GetComponent<PrefabHandler>();

        // Link the asset from the parent to this script for easy instantiating
        prefabLink.Add(parent.p_Player, p_PlayerOnline);

        prefabLink.Add(parent.e_Enemy, e_EnemyOnline);
        prefabLink.Add(parent.e_WardenSensor, e_WardenSensorOnline);
        prefabLink.Add(parent.e_ScreechHead, e_ScreechHeadOnline);
    }
    public GameObject InstantiatePrefab(GameObject obj, Vector3 pos, Quaternion rot, ulong owner = ulong.MaxValue)
    {
        GameObject g = Instantiate(prefabLink[obj], pos, rot);
        if(owner == ulong.MaxValue)
            g.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
        else
            g.GetComponent<NetworkObject>().SpawnWithOwnership(owner);
        spawnedObjects.Add(g.GetComponent<NetworkObject>());
        return g;
    }
    public void DespawnPrefabs()
    {
        foreach(NetworkObject obj in spawnedObjects)
        {
            obj.Despawn();
        }
    }

    public override void OnNetworkSpawn()
    {
        parent.SetNetwork(this);
    }
}
