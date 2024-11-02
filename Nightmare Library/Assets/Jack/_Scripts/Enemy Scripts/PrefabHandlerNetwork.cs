using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using static UnityEditor.PlayerSettings;
using static UnityEngine.Rendering.DebugUI.Table;

[RequireComponent(typeof(PrefabHandler))]
public class PrefabHandlerNetwork : NetworkBehaviour
{
    private static PrefabHandlerNetwork Instance;
    private PrefabHandler parent;

    public Dictionary<GameObject, GameObject> prefabLink = new Dictionary<GameObject, GameObject>();

    [Header("Enemy Prefabs")]
    public GameObject e_WardenSensorOnline;

    private void Awake()
    {
        if (Instance != null)
            Destroy(Instance);
        Instance = this;

        parent = GetComponent<PrefabHandler>();

        // Link the asset from the parent to this script for easy instantiating
        prefabLink.Add(parent.e_WardenSensor, e_WardenSensorOnline);
    }
    public GameObject InstantiatePrefab(GameObject obj, Vector3 pos, Quaternion rot)
    {
        GameObject g = Instantiate(prefabLink[obj], pos, rot);
        g.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
        return g;
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("Test");
        parent.SetNetwork(this);
    }
}
