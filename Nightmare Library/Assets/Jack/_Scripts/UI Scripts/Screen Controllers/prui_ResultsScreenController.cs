using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class prui_ResultsScreenController : ScreenController
{
    [SerializeField]
    private TMP_Text m_ResultText;

    public override void Initialize(UIController parent)
    {
        base.Initialize(parent);

        if(GameController.gameInfo != null)
        {
            GameController.GameInfo info = GameController.gameInfo;

            m_ResultText.text = "Enemies:\n";

            foreach(EnemyPreset e in info.presentEnemies)
            {
                m_ResultText.text += "-" + e.enemyName + "\n";
            }

            m_ResultText.text += "\nResult: ";
            switch (info.endReason)
            {
                case 0:
                    m_ResultText.text += "Shift Over";
                    break;
                case 1:
                    m_ResultText.text += "Monsters Destroyed";
                    break;
                case 2:
                    m_ResultText.text += "Do I need to tell you?";
                    break;
            }
        }
        else
        {
            m_ResultText.text = "Ya Mom";
        }
    }

    public override void ShowScreen()
    {
        base.ShowScreen();
    }
}
