using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Button3D;

public class moui_EvidenceScreenController : ScreenController
{
    [SerializeField]
    private List<Button3D> nameButtons = new List<Button3D>();
    private Dictionary<EnemyPreset, Button3D> presetLink = new Dictionary<EnemyPreset, Button3D>();

    public override void Initialize(UIController parent)
    {
        base.Initialize(parent);

        List<EnemyPreset> presets = GameController.instance.enemyPresets;
        for (int i = 0; i < nameButtons.Count; i++)
        {
            if (presets.Count > i)
            {
                // Set the text of the button to the enemy name
                nameButtons[i].SetText(presets[i].enemyName);

                // Need this to ensure the delegate does not reference the variable i
                EnemyPreset p = presets[i];

                // Set the button to call the click method from the desired enemy
                nameButtons[i].onClick.AddListener(() => ButtonClicked(p));
            }
            else
                nameButtons[i].gameObject.SetActive(false);
        }
    }

    public void ButtonClicked(EnemyPreset p)
    {
        Debug.Log("Test" + p.enemyName);
    }
}
