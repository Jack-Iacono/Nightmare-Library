using BehaviorTree;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pa_Idols : PassiveAttack
{
    protected int maxIdolCount = 3;
    protected static int currentIdolCount = 0;

    private List<IdolController> idolObjects = new List<IdolController>();
    protected int activeIdolObjects = 0;

    private const float baseSpawnTimeMin = 5;
    private const float baseSpawnTimeMax = 7;
    private float spawnTimeMin = baseSpawnTimeMin;
    private float spawnTimeMax = baseSpawnTimeMax;  
    private float spawnTimer = 0;

    public delegate void OnIdolCountChangeDelegate(int idolCount);
    public static event OnIdolCountChangeDelegate OnIdolCountChanged;

    public pa_Idols(Enemy owner) : base(owner)
    {
        name = "Idols";
        toolTip = "It spawns stuff somewhere, not sure that letting them stay around is a good idea";
    }

    public override void Initialize(int level = 1)
    {
        base.Initialize();

        idolObjects = IdolController.GetAllIdols();
        spawnTimer = UnityEngine.Random.Range(spawnTimeMin, spawnTimeMax);
    }

    public override void Update(float dt)
    {
        // Make sure the game isn't paused
        if (!GameController.gamePaused && DeskController.playersAtDesk.Count > 0)
        {
            // Check if there are less than the max amount of idols
            if (currentIdolCount < maxIdolCount)
            {
                if(spawnTimer > 0)
                    spawnTimer -= dt;
                else
                {
                    SpawnIdol();
                    spawnTimer = UnityEngine.Random.Range(spawnTimeMin, spawnTimeMax);
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
        // This ensures that the list cannot be changed once this method starts
        List<PlayerController> playerList = new List<PlayerController>(DeskController.playersAtDesk);
        foreach (PlayerController player in playerList)
        {
            player.ReceiveAttack();
        }

        spawnTimer = UnityEngine.Random.Range(baseSpawnTimeMin - baseSpawnTimeMax, baseSpawnTimeMin + baseSpawnTimeMax);
        ClearIdols();
    }

    public override void OnDestroy()
    {
        currentIdolCount = 0;
        base.OnDestroy();
    }

    protected override void OnLevelChange(int level)
    {
        base.OnLevelChange(level);

        spawnTimeMax = baseSpawnTimeMax / level;
        spawnTimeMin = baseSpawnTimeMin / level;
    }
}
