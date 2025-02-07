using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class moui_TempScreenController : ScreenController
{
    [SerializeField]
    private TMP_Text tempText;
    [SerializeField]
    private TMP_Text stateText;

    private void Awake()
    {
        TempController.OnTempChanged += OnTempChanged;
        TempController.OnTempStateChanged += OnTempStateChanged;
    }

    public override void ShowScreen()
    {
        base.ShowScreen();
        OnTempStateChanged(TempController.tempChangeState);
        OnTempChanged(TempController.currentTemp);
    }

    private void OnTempStateChanged(int state, bool fromServer = false)
    {
        switch(state)
        {
            case 0:
                stateText.text = "Hold";
                break;
            case 1:
                stateText.text = "Heat";
                break;
            case 2:
                stateText.text = "Cool";
                break;
        }
    }
    private void OnTempChanged(int temp)
    {
        tempText.text = temp.ToString() + "\u00B0F";
    }

    public void ChangeTempState()
    {
        TempController.ChangeTempState();
    }
}
