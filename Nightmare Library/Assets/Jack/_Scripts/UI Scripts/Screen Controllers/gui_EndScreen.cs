using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class gui_EndScreen : ScreenController
{
    [SerializeField]
    private TMP_Text endGameText;

    public override void ShowScreen()
    {
        base.ShowScreen();

        string endText = "";
        foreach(Enemy e in Enemy.enemyInstances)
        {
            endText += e.ToString();
        }
        endGameText.text = endText;
        
        Cursor.lockState = CursorLockMode.Confined;
    }
}
