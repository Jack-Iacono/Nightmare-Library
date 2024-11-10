using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pa_Screech : PassiveAttack
{
    private float baseChance = 0.9f;
    private float chanceIncrease = 0.5f;

    // In ticks of the interval, not seconds
    private int coolDownTicks = 3;
    // this is in seconds
    public float attackTime = 10;
    
    private float dist = 2;

    private float intervalTimer = 0;
    private float intervalTime = 1;

    protected Dictionary<ScreechHeadController, HeadData> headControllers = new Dictionary<ScreechHeadController, HeadData>();

    public pa_Screech(Enemy owner) : base(owner)
    {
        intervalTimer = intervalTime;
    }

    public override void Initialize()
    {
        foreach(PlayerController p in PlayerController.playerInstances)
        {
            ScreechHeadController cont = PrefabHandler.Instance.InstantiatePrefab(PrefabHandler.Instance.e_ScreechHead, Vector3.zero, Quaternion.identity).GetComponent<ScreechHeadController>();
            headControllers.Add(cont, new HeadData(baseChance, coolDownTicks));
            cont.Initialize(this, p);
        }
        
        base.Initialize();
    }

    public override void Update(float dt)
    {
        if (intervalTimer > 0)
            intervalTimer -= dt;
        else
        {
            foreach(ScreechHeadController s in headControllers.Keys)
            {
                // Checks for a few things, is the player at the desk, is the head already spawned, has the head already attacked
                if (DeskController.playersAtDesk.Contains(s.targetPlayer) && !s.isSpawned && !s.hasAttacked)
                {
                    float chance = headControllers[s].chance;
                    float cooldown = headControllers[s].cooldown;

                    if (cooldown <= 0)
                    {
                        float rand = Random.Range(0, 1f);
                        if (rand < chance)
                        {
                            //Spawn
                            headControllers[s].SetChance(baseChance);
                            headControllers[s].SetCooldown(coolDownTicks);

                            s.SpawnHead(GetRandomOffset());
                        }
                        else
                        {
                            headControllers[s].IncreaseChance(chanceIncrease);
                        }
                    }
                    else
                        headControllers[s].TickCooldown();
                }
            }

            intervalTimer = intervalTime;
        }
    }

    public void AttackPlayer(PlayerController player)
    {
        // Remove to actually attack player
        //player.ReceiveAttack();
    }

    public Vector3 GetRandomOffset()
    {
        float yRot = Random.Range(0, 360);
        float xRot = Random.Range(-45, 45);

        float a = Mathf.Sin(yRot * Mathf.Deg2Rad);
        float b = Mathf.Cos(yRot * Mathf.Deg2Rad);

        float y = Mathf.Sin(xRot * Mathf.Deg2Rad);
        float z = Mathf.Cos(xRot * Mathf.Deg2Rad) * dist;

        return new Vector3(a * z, y, b * z);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    protected class HeadData
    {
        public float chance;
        public int cooldown;

        public HeadData(float chance, int cooldown)
        {
            this.chance = chance;
            this.cooldown = cooldown;
        }

        public void SetChance(float newChance)
        {
            chance =  newChance;
        }
        public void IncreaseChance(float increase)
        {
            chance += increase;
        }

        public void SetCooldown(int newCooldown)
        {
            cooldown = newCooldown;
        }
        public void TickCooldown()
        {
            cooldown--;
            Debug.Log(cooldown);
        }
    }
}

