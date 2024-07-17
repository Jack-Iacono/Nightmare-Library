using BehaviorTree;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pa_Idols : PassiveAttack
{
    TaskSpawnIdols idolSpawner;
    private int maxIdolCount = 7;

    private List<IdolController> idolObjects = new List<IdolController>();
    private int activeIdolObjects = 0;

    public pa_Idols(Enemy owner) : base(owner)
    {
        idolSpawner = new TaskSpawnIdols(3, 0.5f);
        TaskSpawnIdols.OnIdolCountChanged += OnIdolCountChanged;

        idolObjects = DeskController.instance.GetIdolControllers(idolSpawner);
    }

    protected override Node SetupTree()
    {
        // Establises the Behavior Tree and its logic
        Node root = new Selector(new List<Node>()
        {
            new Sequence(new List<Node>()
            {
                new CheckIdolCount(maxIdolCount),
                new TaskAttackPlayerPassive()
            }),
            idolSpawner
        });

        foreach(IdolController i in idolObjects)
        {
            Debug.Log(i == null);
        }

        return root;
    }

    private void OnIdolCountChanged(int e)
    {
        if (e > activeIdolObjects)
        {
            for (int i = 0; i < idolObjects.Count; i++)
            {
                Debug.Log("i: " + i);
                Debug.Log(idolObjects[i] == null);
                if (!idolObjects[i].gameObject.activeInHierarchy)
                {
                    idolObjects[i].AddIdol();
                    break;
                }
            }
        }

        activeIdolObjects = e;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        Debug.Log("Unregistering");
        TaskSpawnIdols.OnIdolCountChanged -= OnIdolCountChanged;
    }
}
