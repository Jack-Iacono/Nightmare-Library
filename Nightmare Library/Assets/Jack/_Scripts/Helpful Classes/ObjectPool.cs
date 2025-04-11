using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;

public class ObjectPool
{
    private Dictionary<string, List<GameObject>> pooledObjects = new Dictionary<string, List<GameObject>>();
    private bool spawnOfflineOverride = false;

    public ObjectPool() { }

    public void PoolObject(GameObject gObject, int count, bool spawnOfflineOverride = false)
    {
        GameObject p = new GameObject("Pooled Object: " + gObject.name);
        this.spawnOfflineOverride = spawnOfflineOverride;

        // Creates an empty list
        List<GameObject> list = new List<GameObject>();

        // Populates the list with the right amount of gameobjects
        for (int i = 0; i < count; i++)
        {
            var inst = InstantiateObject(gObject, p.transform);
            inst.name += " " + i;
            list.Add(inst);
        }

        // Adds the item to the dictionary
        pooledObjects.Add(gObject.name, list);
    }

    public GameObject AddObject(GameObject gObject)
    {
        GameObject newObject = InstantiateObject(gObject, pooledObjects[gObject.name][0].transform);
        pooledObjects[gObject.name].Add(newObject);
        return newObject;
    }
    private GameObject InstantiateObject(GameObject obj, Transform parent)
    {
        GameObject g;
        if (!spawnOfflineOverride)
            g = PrefabHandler.Instance.InstantiatePrefab(obj, parent.position, parent.rotation);
        else
            g = PrefabHandler.Instance.InstantiatePrefabOffline(obj, parent.position, parent.rotation);

        g.SendMessage("Initialize", SendMessageOptions.DontRequireReceiver);

        if (NetworkConnectionController.IsRunning)
            g.SendMessage("OnPoolSpawn", SendMessageOptions.DontRequireReceiver);

        g.SetActive(false);
        return g;
    }

    public void AddObjectToPool(GameObject pool, GameObject addedObject)
    {
        if(pooledObjects.ContainsKey(pool.name))
            pooledObjects[pool.name].Add(addedObject);
        else
        {
            pooledObjects.Add(pool.name, new List<GameObject> { addedObject });
        }
    } 

    public GameObject GetObject(GameObject g)
    {
        List<GameObject> list = pooledObjects[g.name];

        foreach(GameObject obj in list)
        {
            if (!obj.activeInHierarchy)
                return obj;
        }

        return AddObject(g);
    }
    public List<GameObject> GetPool(GameObject g)
    {
        return pooledObjects[g.name];
    }

    public int ObjectCount(GameObject g)
    {
        return pooledObjects[g.name].Count;
    }

    public void CleanupPool()
    {
        // Despawn all objects if they need to be despawned
        foreach(List<GameObject> list in pooledObjects.Values)
        {
            foreach (GameObject obj in list)
            {
                if(obj != null && PrefabHandler.Instance != null)
                {
                    PrefabHandler.Instance.CleanupGameObject(obj);
                    PrefabHandler.Instance.DestroyGameObject(obj);
                }
                    
            }
        }

        pooledObjects = new Dictionary<string, List<GameObject>>();
    }
}
