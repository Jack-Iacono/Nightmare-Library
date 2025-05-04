using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pa_Temps : PassiveAttack
{
    protected Vector2 safeTempRange = new Vector2(32, 98);
    private bool upOrDown = false;

    protected const int tempTickAvg = 3;
    protected const int tempTickDev = 2;

    private const float baseTickTimeMin = 1;
    private const float baseTickTimeMax = 4;
    private float tickTimeMin = baseTickTimeMin;
    private float tickTimeMax = baseTickTimeMax;
    private float tickTimer = 0;

    public pa_Temps(Enemy owner) : base(owner)
    {
        name = "Temperature";
        toolTip = "Wear a coat, but only if it's cold";

        // Gets a random movement for the temp
        upOrDown = Random.Range(0,2) == 0 ? true : false;
    }

    public override void Initialize(int level = 1)
    {
        base.Initialize();
        tickTimer = Random.Range(tickTimeMin - tickTimeMax, tickTimeMin + tickTimeMax);
    }

    public override void Update(float dt)
    {
        // Make sure the game isn't paused
        if (DeskController.playersAtDesk.Count > 0)
        {
            if (tickTimer > 0)
                tickTimer -= dt;
            else
            {
                if (upOrDown)
                    TempController.RaiseTemp(Random.Range(tempTickAvg - tempTickDev, tempTickAvg + tempTickDev));
                else
                    TempController.LowerTemp(Random.Range(tempTickAvg - tempTickDev, tempTickAvg + tempTickDev));

                if (TempController.currentTemp < safeTempRange.x || TempController.currentTemp > safeTempRange.y)
                {
                    float diff = (32 - Mathf.Min(Mathf.Abs(TempController.currentTemp - safeTempRange.x), Mathf.Abs(safeTempRange.y - TempController.currentTemp))) / 32;
                    if (Random.Range(0f, 1f) < diff)
                        AttackPlayer();
                }

                tickTimer = Random.Range(tickTimeMin, tickTimeMax);
            }
        }
    }

    protected void AttackPlayer()
    {
        foreach (PlayerController player in DeskController.playersAtDesk)
        {
            player.ChangeAliveState(false);
        }

        tickTimer = UnityEngine.Random.Range(tickTimeMin - tickTimeMax, tickTimeMin + tickTimeMax);
        TempController.ResetTemp();
    }

    public override void OnDestroy()
    {
        TempController.currentTemp = 0;
        base.OnDestroy();
    }

    protected override void OnLevelChange(int level)
    {
        base.OnLevelChange(level);

        tickTimeMin = baseTickTimeMin;
        tickTimeMax = baseTickTimeMax;
    }
}
