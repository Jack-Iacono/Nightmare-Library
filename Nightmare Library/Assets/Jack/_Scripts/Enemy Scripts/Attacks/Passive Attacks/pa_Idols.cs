using BehaviorTree;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pa_Idols : PassiveAttack
{
    protected int maxIdolCount = 7;
    protected static int currentIdolCount = 0;

    private List<IdolController> idolObjects = new List<IdolController>();
    protected int activeIdolObjects = 0;

    private float spawnTimeAvg = 4;
    private float spawnTimeDev = 1;
    private float spawnTimer = 0;

    public delegate void OnIdolCountChangeDelegate(int idolCount);
    public static event OnIdolCountChangeDelegate OnIdolCountChanged;

    public pa_Idols(Enemy owner) : base(owner)
    {
    }

    public override void Initialize()
    {
        base.Initialize();

        idolObjects = IdolController.GetAllIdols();
        spawnTimer = UnityEngine.Random.Range(spawnTimeAvg - spawnTimeDev, spawnTimeAvg + spawnTimeDev);
    }

    public override void Update(float dt)
    {
        // Make sure the game isn't paused
        if (!GameController.gamePaused)
        {
            // Check if there are less than the max amount of idols
            if (currentIdolCount < maxIdolCount)
            {
                if(spawnTimer > 0)
                    spawnTimer -= dt;
                else
                {
                    SpawnIdol();
                    spawnTimer = UnityEngine.Random.Range(spawnTimeAvg - spawnTimeDev, spawnTimeAvg + spawnTimeDev);
                }
            }
            else
            {
                AttackPlayer();
            }
        }
    }

    protected void SpawnIdol()
    {
        currentIdolCount++;

        for(int i = 0; i < idolObjects.Count; i++)
        {
            if (!idolObjects[i].isActive)
            {
                idolObjects[i].Activate();
                OnIdolCountChanged?.Invoke(currentIdolCount);
                break;
            }
        }
    }
    public static void RemoveIdol()
    {
        currentIdolCount--;
        OnIdolCountChanged?.Invoke(currentIdolCount);
    }
    protected void ClearIdols()
    {
        currentIdolCount = 0;
        foreach(IdolController i in idolObjects)
        {
            i.Deactivate();
        }
    }

    protected void AttackPlayer()
    {
        foreach (PlayerController player in DeskController.playersAtDesk)
        {
            player.ReceiveAttack();
        }

        spawnTimer = UnityEngine.Random.Range(spawnTimeAvg - spawnTimeDev, spawnTimeAvg + spawnTimeDev);
        ClearIdols();
    }

    public override void OnDestroy()
    {
        currentIdolCount = 0;
        base.OnDestroy();
    }
}
