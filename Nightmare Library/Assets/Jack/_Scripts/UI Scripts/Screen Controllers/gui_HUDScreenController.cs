using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class gui_HUDScreenController : ScreenController
{
    [SerializeField]
    private Image reticle;
    [SerializeField]
    private Sprite normalReticle;
    [SerializeField]
    private Sprite clickReticle;
    [SerializeField]
    private Sprite pickupReticle;

    private void Start()
    {
        PlayerInteractionController.onItemSightChange += OnItemSightChanged;

        reticle.sprite = normalReticle;
    }

    private string FloatToTime(float time)
    {
        string timeString = string.Empty;

        int hour = Mathf.FloorToInt(time / 3600);
        time -= hour * 3600;
        int min = Mathf.FloorToInt(time / 60);
        time -= min * 60;
        int sec = (int)time;

        if (hour < 10)
            timeString += "0" + hour + ":";
        else
            timeString += hour.ToString() + ":";

        if (min < 10)
            timeString += "0" + min + ":";
        else
            timeString += min.ToString() + ":";

        if (sec < 10)
            timeString += "0" + sec;
        else
            timeString += sec.ToString();

        return timeString;
    }

    private void OnItemSightChanged(int interactionType)
    {
        switch (interactionType)
        {
            case -1:
                reticle.sprite = normalReticle;
                break;
            case 0:
                reticle.sprite = clickReticle;
                break;
            case 1:
                reticle.sprite = pickupReticle;
                break;
        }
    }

    private void OnDestroy()
    {
        PlayerInteractionController.onItemSightChange -= OnItemSightChanged;
    }
}
