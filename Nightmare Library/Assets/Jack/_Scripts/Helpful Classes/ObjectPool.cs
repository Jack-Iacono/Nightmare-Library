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
        GameObject newObject = InstantiateObject(gObject, pooledObjects[gObject.name][0].transform.parent);
        pooledObjects[gObject.name].Add(newObject);
        return newObject;
    }
    private GameObject InstantiateObject(GameObject obj, Transform parent, bool setActive = false)
    {
        //GameObject g = GameObject.Instantiate(obj, parent);
        GameObject g = PrefabHandler.Instance.InstantiatePrefab(obj, parent.position, parent.rotation);
        g.SendMessage("Initialize", SendMessageOptions.DontRequireReceiver);
        g.SetActive(setActive);
        return g;
    }

    public GameObject GetObject(GameObject g)
    {
        Debug.Log(g.name);
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
