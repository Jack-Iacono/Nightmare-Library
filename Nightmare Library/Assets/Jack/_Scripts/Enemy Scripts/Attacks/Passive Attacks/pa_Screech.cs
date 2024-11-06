using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pa_Screech : PassiveAttack
{
    public float baseChance { get; private set; } = 0.9f;
    public float chanceIncrease { get; private set; } = 0.5f;
    
    public float dist { get; private set; } = 2;

    private float intervalTimer = 0;
    private float intervalTime = 1;

    private List<ScreechHeadController> headControllers = new List<ScreechHeadController>();

    // Variable here for enemy

    public pa_Screech(Enemy owner) : base(owner)
    {
        intervalTimer = intervalTime;
    }

    public override void Initialize()
    {
        foreach(PlayerController p in PlayerController.playerInstances)
        {
            ScreechHeadController cont = PrefabHandler.Instance.InstantiatePrefab(PrefabHandler.Instance.e_ScreechHead, Vector3.zero, Quaternion.identity).GetComponent<ScreechHeadController>();
            headControllers.Add(cont);
            cont.Initialize(this, p);
        }
        
        base.Initialize();
    }

    public override void Update(float dt)
    {
        /*
        if (!isSpawned && DeskController.playersAtDesk.Contains(PlayerController.playerInstances[0]))
        {
            if (cooldownTimer <= 0)
            {
                if (intervalTimer > 0)
                    intervalTimer -= dt;
                else
                {
                    float rand = Random.Range(0, 1f);
                    if (rand < currentChance)
                    {
                        //Spawn
                        currentChance = baseChance;
                        cooldownTimer = 0;
                        isSpawned = true;

                        SpawnHead();
                    }
                    else
                    {
                        currentChance += chanceIncrease;
                    }

                    intervalTimer = intervalTime;
                }
            }
            else
                cooldownTimer -= dt;
        }
        */
    }

    protected void AttackPlayer()
    {
        foreach (PlayerController player in DeskController.playersAtDesk)
        {
            player.ReceiveAttack();
        }

    }

    public void SpawnHead(ScreechHeadController controller)
    {
        float yRot = Random.Range(0, 360);
        float xRot = Random.Range(-45, 45);

        float a = Mathf.Sin(yRot * Mathf.Deg2Rad);
        float b = Mathf.Cos(yRot * Mathf.Deg2Rad);

        float y = Mathf.Sin(xRot * Mathf.Deg2Rad);
        float z = Mathf.Cos(xRot * Mathf.Deg2Rad) * dist;

        Vector3 offset = new Vector3(a * z, y, b * z);

        controller.SpawnHead(offset);
    }

    public override void OnDestroy()
    {
        TempController.currentTemp = 0;
        base.OnDestroy();
    }
}
