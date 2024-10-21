using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempController : MonoBehaviour
{
    public const int minTemp = 0;
    public const int maxTemp = 130;

    public static int currentTemp = maxTemp / 2;
    private static int targetTemp = 0;

    protected const int tempTickAvg = 2;
    protected const int tempTickDev = 1;

    private const float updateTickAvg = 4;
    private const float updateTickDev = 1;
    private float updateTick = 0;

    private const float tempTickTime = 1;
    private float tempTick = tempTickTime;

    // 0: None, 1: Raising, 2: Lowering
    public static int tempChangeState { get; private set; } = 0;

    public delegate void OnTempChangeDelegate(int temp);
    public static event OnTempChangeDelegate OnTempChanged;
    public delegate void OnTempStateChangeDelegate(int state, bool fromServer);
    public static event OnTempStateChangeDelegate OnTempStateChanged;

    private void Start()
    {
        targetTemp = currentTemp;
    }
    public void Update()
    {
        // Make sure the game isn't paused
        if (!GameController.gamePaused)
        {
            // Controls the movement of the target temperature based on external factors
            if (updateTick > 0)
                updateTick -= Time.deltaTime;
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

                updateTick = Random.Range(updateTickAvg - updateTickDev, updateTickAvg + updateTickDev);
            }

            // Moves the temperature smoothly
            if(tempTick > 0)
                tempTick -= Time.deltaTime;
            else
            {
                currentTemp = (int)Mathf.MoveTowards(currentTemp, targetTemp, 1);
                OnTempChanged?.Invoke(currentTemp);
                tempTick = tempTickTime;
            }
        }
    }

    protected void WaverTemp()
    {
        targetTemp = Mathf.Clamp(targetTemp - Random.Range(-tempTickAvg - tempTickDev, tempTickAvg + tempTickDev), minTemp, maxTemp);
    }
    protected void LowerTemp()
    {
        targetTemp = Mathf.Clamp(targetTemp - Random.Range(tempTickAvg - tempTickDev, tempTickAvg + tempTickDev), minTemp, maxTemp);
    }
    protected void RaiseTemp()
    {
        targetTemp = Mathf.Clamp(targetTemp + Random.Range(tempTickAvg - tempTickDev, tempTickAvg + tempTickDev), minTemp, maxTemp);
        
    }

    public static void LowerTemp(int amount)
    {
        targetTemp = Mathf.Clamp(targetTemp - amount, minTemp, maxTemp);
    }
    public static void RaiseTemp(int amount)
    {
        targetTemp = Mathf.Clamp(targetTemp + amount, minTemp, maxTemp);
    }

    public static void SetTemp(int temp)
    {
        targetTemp = temp;
        currentTemp = temp;
        OnTempChanged?.Invoke(currentTemp);
    }
    public static void SetState(int state)
    {
        tempChangeState = state;
        OnTempStateChanged?.Invoke(tempChangeState, true);
    }
    public static void ResetTemp()
    {
        currentTemp = Mathf.RoundToInt(maxTemp / 2);
        targetTemp = currentTemp;
    }

    public static void ChangeTempState()
    {
        tempChangeState = (tempChangeState + 1) % 3;
        OnTempStateChanged?.Invoke(tempChangeState, false);    
    }
}
