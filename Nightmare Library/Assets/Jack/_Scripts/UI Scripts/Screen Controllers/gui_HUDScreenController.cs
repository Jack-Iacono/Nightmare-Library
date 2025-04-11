using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class gui_HUDScreenController : ScreenController
{
    [SerializeField]
    private TMP_Text timerText;
    [SerializeField]
    private TMP_Text inventoryText;

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
        InventoryController.instance.onHeldItemChanged += OnInventoryHeldItemChanged;
        PlayerInteractionController.onItemSightChange += OnItemSightChanged;

        inventoryText.text = "Empty";
        reticle.sprite = normalReticle;
    }

    

    private void Update()
    {
        if (GameController.instance != null)
            timerText.text = FloatToTime(GameController.instance.gameTimer);
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
    private void OnInventoryHeldItemChanged(InventoryItem item)
    {
        
    }
    private void OnInventoryHeldItemChanged(HoldableItem holdable)
    {
        if (holdable != null)
            inventoryText.text = holdable.name;
        else
            inventoryText.text = "Empty";
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
