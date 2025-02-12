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
            List<EnemyPreset> usedPresets = new List<EnemyPreset>();

            for(int i = 0; i < guesses.Count; i++)
            {
                
                if (presets.Contains(guesses[i]))
                {
                    if (usedPresets.Contains(guesses[i]))
                        guessResults[i].text = guesses[i].enemyName + ": Nice Try, I'm not paying you for this one";
                    else
                        guessResults[i].text = guesses[i].enemyName + ": Accepted";
                }
                else
                {
                    if (usedPresets.Contains(guesses[i]))
                        guessResults[i].text = guesses[i].enemyName + ": Why would you guess this twice dumbass";
                    else
                        guessResults[i].text = guesses[i].enemyName + ": Rejected";
                }
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
