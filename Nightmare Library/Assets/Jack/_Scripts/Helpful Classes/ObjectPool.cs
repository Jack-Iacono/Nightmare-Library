using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;

public class ObjectPool
{
    private Dictionary<string, List<GameObject>> pooledObjects = new Dictionary<string, List<GameObject>>();

    public ObjectPool() { }

    public void PoolObject(GameObject gObject, int count, Transform parent, bool setActive = false)
    {
        GameObject p = new GameObject("Pooled Object: " + gObject.name);
        p.transform.parent = parent;

        // Creates an empty list
        List<GameObject> list = new List<GameObject>();

        // Populates the list with the right amount of gameobjects
        for (int i = 0; i < count; i++)
        {
            var inst = InstantiateObject(gObject, p.transform, setActive);
            inst.name += " " + i;
            list.Add(inst);
        }

        // Adds the item to the dictionary
        pooledObjects.Add(gObject.name, list);
    }
    public void PoolObject(GameObject gObject, int count, bool setActive = false)
    {
        GameObject p = new GameObject("Pooled Object: " + gObject.name);

        // Creates an empty list
        List<GameObject> list = new List<GameObject>();

        // Populates the list with the right amount of gameobjects
        for (int i = 0; i < count; i++)
        {
            var inst = InstantiateObject(gObject, p.transform, setActive);
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
    private GameObject InstantiateObject(GameObject obj, Transform parent, bool setActive = false)
    {
        GameObject g = PrefabHandler.Instance.InstantiatePrefab(obj, parent.position, parent.rotation);

        g.SendMessage("Initialize", SendMessageOptions.DontRequireReceiver);
        if (NetworkConnectionController.IsRunning)
            g.SendMessage("OnPoolSpawn", SendMessageOptions.DontRequireReceiver);
        g.SetActive(setActive);
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
}
