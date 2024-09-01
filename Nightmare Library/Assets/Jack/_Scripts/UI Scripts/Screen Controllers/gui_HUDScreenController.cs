using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class gui_HUDScreenController : ScreenController
{
    [SerializeField]
    private TMP_Text timerText;
    [SerializeField]
    private TMP_Text inventoryText;

    private void Start()
    {
        InventoryController.Instance.onHeldItemChanged += OnInventoryHeldItemChanged;
    }

    private void Update()
    {
        if (GameController.instance != null)
            timerText.text = FloatToTime(GameController.instance.gameTimer);
    }

    private string FloatToTime(float time)
    {
        string timeString = string.Empty;

        int min = Mathf.FloorToInt(time / 60);
        int sec = (int)(time % 60);

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
        if (item.realObject != null)
            inventoryText.text = item.realObject.name;
        else
            inventoryText.text = "Empty";
    }
}
