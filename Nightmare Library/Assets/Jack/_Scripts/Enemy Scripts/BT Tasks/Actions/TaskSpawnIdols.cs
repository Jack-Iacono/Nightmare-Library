using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;
using System;

public class TaskSpawnIdols : Node
{
    public const string IDOL_KEY = "idolCountKey";
    public static int currentIdolCount;

    private float avgSpawnTime;
    private float spawnTimeDeviation;
    private float currentSpawnTimer = 0;

    public event EventHandler<int> OnIdolCountChanged;

    public TaskSpawnIdols(float avgSpawnTime, float spawnTimeDeviation)
    {
        this.avgSpawnTime = avgSpawnTime;
        this.spawnTimeDeviation = spawnTimeDeviation;

        currentSpawnTimer = UnityEngine.Random.Range(avgSpawnTime - spawnTimeDeviation, avgSpawnTime + spawnTimeDeviation);
    }

    public override Status Check(float dt)
    {
        if(DeskController.playersAtDesk.Count > 0) 
        {
            if (currentSpawnTimer <= 0)
            {
                // Start a new timer with a random deviation
                currentSpawnTimer = UnityEngine.Random.Range(avgSpawnTime - spawnTimeDeviation, avgSpawnTime + spawnTimeDeviation);

                AddIdol();
            }
            else
            {
                currentSpawnTimer -= dt;
            }
        }

        status = Status.RUNNING;
        return status;
    }

    public void AddIdol()
    {
        currentIdolCount++;
        SetRootData(IDOL_KEY, currentIdolCount);

        OnIdolCountChanged?.Invoke(this, currentIdolCount);
    }
    public void RemoveIdol()
    {
        currentIdolCount--;
        SetRootData(IDOL_KEY, currentIdolCount);

        OnIdolCountChanged?.Invoke(this, currentIdolCount);
    }
}
