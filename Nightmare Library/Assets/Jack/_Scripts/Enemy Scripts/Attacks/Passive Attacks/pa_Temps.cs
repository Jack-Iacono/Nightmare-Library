using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pa_Temps : PassiveAttack
{
    protected Vector2 safeTempRange = new Vector2(32, 98);
    private bool upOrDown = false;

    protected const int tempTickAvg = 3;
    protected const int tempTickDev = 2;

    private const float tickTimeAvg = 4;
    private const float tickTimeDev = 1;
    private float tickTimer = 0;

    public pa_Temps(Enemy owner) : base(owner)
    {
        // Gets a random movement for the temp
        upOrDown = Random.Range(0,2) == 0 ? true : false;
    }

    public override void Initialize()
    {
        base.Initialize();
        tickTimer = Random.Range(tickTimeAvg - tickTimeDev, tickTimeAvg + tickTimeDev);
    }

    public override void Update(float dt)
    {
        // Make sure the game isn't paused
        if (!GameController.gamePaused && DeskController.playersAtDesk.Count > 0)
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

                tickTimer = Random.Range(tickTimeAvg - tickTimeDev, tickTimeAvg + tickTimeDev);
            }
        }
    }

    protected void AttackPlayer()
    {
        foreach (PlayerController player in DeskController.playersAtDesk)
        {
            player.ReceiveAttack();
        }

        tickTimer = UnityEngine.Random.Range(tickTimeAvg - tickTimeDev, tickTimeAvg + tickTimeDev);
        TempController.ResetTemp();
    }

    public override void OnDestroy()
    {
        TempController.currentTemp = 0;
        base.OnDestroy();
    }
}
