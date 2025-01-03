using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gui_EndScreen : ScreenController
{
    [SerializeField]
    private GameObject endGameButton;

    public override void ShowScreen()
    {
        base.ShowScreen();

        if (NetworkConnectionController.HasAuthority)
            endGameButton.SetActive(true);
        else
            endGameButton.SetActive(false);
        
        Cursor.lockState = CursorLockMode.Confined;
    }
    public void EndGameButtonClick()
    {
        GameController.ReturnToMenu();
    }
}
