using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pa_Screech : PassiveAttack
{
    private float currentChance = 0;
    private float baseChance = 0.1f;
    private float chanceIncrease = 0.05f;

    private float intervalTimer = 0;
    private float intervalTime = 4;

    private float cooldownTimer = 0;
    private float cooldownTime = 10;

    private bool isSpawned = false;

    // Variable here for enemy

    public pa_Screech(Enemy owner) : base(owner)
    {
        currentChance = baseChance;
        cooldownTimer = cooldownTime;
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float dt)
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
                }
                else
                {
                    currentChance += chanceIncrease;
                }

                intervalTimer = intervalTime;
            }
        }
        else if(!isSpawned)
            cooldownTimer -= dt;
    }

    protected void AttackPlayer()
    {
        foreach (PlayerController player in DeskController.playersAtDesk)
        {
            player.ReceiveAttack();
        }

    }

    public void SpawnHeads()
    {

    }

    public override void OnDestroy()
    {
        TempController.currentTemp = 0;
        base.OnDestroy();
    }
}
