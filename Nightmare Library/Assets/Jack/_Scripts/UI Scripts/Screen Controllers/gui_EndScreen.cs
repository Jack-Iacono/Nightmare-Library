using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class gui_EndScreen : ScreenController
{
    [SerializeField]
    private GameObject endGameButton;
    [SerializeField]
    private TMP_Text endGameText;

    public override void ShowScreen()
    {
        base.ShowScreen();

        if (NetworkConnectionController.HasAuthority)
            endGameButton.SetActive(true);
        else
            endGameButton.SetActive(false);

        string endText = "";
        foreach(Enemy e in Enemy.enemyInstances)
        {
            endText += e.ToString();
        }
        endGameText.text = endText;
        
        Cursor.lockState = CursorLockMode.Confined;
    }
    public void EndGameButtonClick()
    {
        GameController.ReturnToMenu();
    }
}
