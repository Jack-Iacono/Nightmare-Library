using BehaviorTree;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pa_Idols : PassiveAttack
{
    TaskSpawnIdols idolSpawner;
    private int maxIdolCount = 7;

    private List<GameObject> idolObjects = new List<GameObject>();

    public pa_Idols(Enemy owner) : base(owner)
    {
        idolSpawner = new TaskSpawnIdols(3, 0.5f);
        idolSpawner.OnIdolCountChanged += OnIdolCountChanged;

        idolObjects = DeskController.instance.SpawnIdols(maxIdolCount, idolSpawner);

        foreach (GameObject obj in idolObjects)
        {
            obj.SetActive(false);
        }
    }

    protected override Node SetupTree()
    {
        // Establises the Behavior Tree and its logic
        Node root = new Selector(new List<Node>()
        {
            new Sequence(new List<Node>()
            {
                new CheckIdolCount(maxIdolCount)
            }),
            idolSpawner
        });

        return root;
    }

    private void OnIdolCountChanged(object sender, int e)
    {
        for(int i = 0; i < idolObjects.Count; i++)
        {
            if(i < e)
            {
                idolObjects[i].SetActive(true);
            }
            else
            {
                idolObjects[i].SetActive(false);
            }
        }
    }
}
