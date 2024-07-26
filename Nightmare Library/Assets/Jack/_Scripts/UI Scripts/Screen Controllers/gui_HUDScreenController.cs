using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class gui_HUDScreenController : ScreenController
{
    [SerializeField]
    private TMP_Text timerText;

    private void Update()
    {
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

}
