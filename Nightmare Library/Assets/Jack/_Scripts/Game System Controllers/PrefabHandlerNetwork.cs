using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(PrefabHandler))]
public class PrefabHandlerNetwork : NetworkBehaviour
{
    public static PrefabHandlerNetwork Instance;
    private PrefabHandler parent;

    private static List<NetworkObject> spawnedObjects = new List<NetworkObject>();

    private void Awake()
    {
        if (!NetworkConnectionController.connectedToLobby)
        {
            Destroy(this);
            Destroy(GetComponent<NetworkObject>());
        }

        if (Instance != null)
            Destroy(Instance);
        Instance = this;

        parent = GetComponent<PrefabHandler>();
    }
    public void InstantiatePrefab(GameObject obj, ulong owner = ulong.MaxValue)
    {
        if(owner == ulong.MaxValue)
            obj.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
        else
            obj.GetComponent<NetworkObject>().SpawnWithOwnership(owner);

        // Send a message to the object saying that it is spawned
        obj.SendMessage("OnObjectSpawn", SendMessageOptions.DontRequireReceiver);

        spawnedObjects.Add(obj.GetComponent<NetworkObject>());
    }

    public static void AddSpawnedPrefab(NetworkObject obj)
    {
        if(!spawnedObjects.Contains(obj))
            spawnedObjects.Add(obj);
    }
    public void DespawnPrefabs()
    {
        if (NetworkManager.IsServer)
        {
            foreach (NetworkObject obj in spawnedObjects)
            {
                if (obj != null && obj.IsSpawned)
                    obj.Despawn();
            }
        }
        
        spawnedObjects.Clear();
    }
    public void DespawnPrefab(GameObject g)
    {
        if (NetworkManager.IsServer)
        {
            NetworkObject obj;
            if (g.TryGetComponent<NetworkObject>(out obj))
            {
                if (obj != null)
                    obj.Despawn();
            }
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        spawnedObjects.Clear();
    }
    public override void OnNetworkSpawn()
    {
        parent.SetNetwork(this);
    }
}
