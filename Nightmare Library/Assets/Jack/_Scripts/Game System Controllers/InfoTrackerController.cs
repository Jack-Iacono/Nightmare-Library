using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoTrackerController : MonoBehaviour
{
    [NonSerialized]
    private int[] monsterGuesses = new int[GameController.enemyCount];
    [SerializeField]
    private GameObject monsterGuessItem;
    [SerializeField]
    private GameObject guessitemParent;

    private Dictionary<string, int> monsterNames = new Dictionary<string, int>();

    // Start is called before the first frame update
    void Start()
    {
        // Initializes the dictionary for players guessing
        for(int i = 0; i < GameController.instance.enemyPresets.Count; i++)
        {
            monsterNames.Add(GameController.instance.enemyPresets[i].name, i);
            Debug.Log(GameController.instance.enemyPresets[i].name);
        }
    }


}
