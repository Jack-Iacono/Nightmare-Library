using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class prui_ResultsScreenController : ScreenController
{
    [Space(10)]
    [SerializeField]
    private List<TMP_Text> guessResults = new List<TMP_Text>();

    public override void Initialize(UIController parent)
    {
        base.Initialize(parent);

        foreach(TMP_Text text in guessResults)
        {
            text.text = "";
        }

        if(GameController.roundResults != null)
        {
            List<EnemyPreset> presets = GameController.roundResults.presentEnemies;
            List<EnemyPreset> guesses = GameController.roundResults.enemyGuesses;

            for(int i = 0; i < presets.Count; i++)
            {
                if (guesses.Contains(presets[i]))
                    guessResults[i].text = presets[i].enemyName + ": Detected";
                else
                    guessResults[i].text = presets[i].enemyName + ": Not Detected";
            }
        }
        else
        {
            guessResults[0].text = "You have to play the game first";
        }
    }

    public override void ShowScreen()
    {
        base.ShowScreen();
    }
}
