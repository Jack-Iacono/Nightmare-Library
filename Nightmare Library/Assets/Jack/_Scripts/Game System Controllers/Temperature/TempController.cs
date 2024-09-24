using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempController : MonoBehaviour
{
    public const int minTemp = 0;
    public const int maxTemp = 130;

    public static int currentTemp = maxTemp / 2;

    protected const int tempTickAvg = 2;
    protected const int tempTickDev = 1;

    private const float tickTimeAvg = 4;
    private const float tickTimeDev = 1;
    private float tickTimer = 0;

    // 0: None, 1: Raising, 2: Lowering
    public static int tempChangeState { get; private set; } = 0;

    public delegate void OnTempChangeDelegate(int temp);
    public static event OnTempChangeDelegate OnTempChanged;
    public static event OnTempChangeDelegate OnTempStateChanged;

    public void Update()
    {
        // Make sure the game isn't paused
        if (!GameController.gamePaused)
        {
            if (tickTimer > 0)
                tickTimer -= Time.deltaTime;
            else
            {
                switch (tempChangeState)
                {
                    case 0:
                        WaverTemp();
                        break;
                    case 1:
                        RaiseTemp();
                        break;
                    case 2:
                        LowerTemp();
                        break;
                }

                tickTimer = Random.Range(tickTimeAvg - tickTimeDev, tickTimeAvg + tickTimeDev);
            }
        }
    }

    protected void WaverTemp()
    {
        currentTemp = Mathf.Clamp(currentTemp - Random.Range(-tempTickAvg - tempTickDev, tempTickAvg + tempTickDev), minTemp, maxTemp);
        OnTempChanged?.Invoke(currentTemp);
    }
    protected void LowerTemp()
    {
        currentTemp = Mathf.Clamp(currentTemp - Random.Range(tempTickAvg - tempTickDev, tempTickAvg + tempTickDev), minTemp, maxTemp);
        OnTempChanged?.Invoke(currentTemp);
    }
    protected void RaiseTemp()
    {
        currentTemp = Mathf.Clamp(currentTemp + Random.Range(tempTickAvg - tempTickDev, tempTickAvg + tempTickDev), minTemp, maxTemp);
        OnTempChanged?.Invoke(currentTemp);
    }

    public static void LowerTemp(int amount)
    {
        currentTemp = Mathf.Clamp(currentTemp - amount, minTemp, maxTemp);
        OnTempChanged?.Invoke(currentTemp);
    }
    public static void RaiseTemp(int amount)
    {
        currentTemp = Mathf.Clamp(currentTemp + amount, minTemp, maxTemp);
        OnTempChanged?.Invoke(currentTemp);
    }

    public static void ResetTemp()
    {
        currentTemp = Mathf.RoundToInt(maxTemp / 2);
    }

    public static void ChangeTempState()
    {
        tempChangeState = (tempChangeState + 1) % 3;
        OnTempStateChanged?.Invoke(tempChangeState);    
    }
}
