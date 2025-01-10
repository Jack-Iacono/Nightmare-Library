using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

using static Button3D;
using static EnemyPreset;

public class moui_EvidenceScreenController : ScreenController
{
    [SerializeField]
    private List<Button3D> nameButtons = new List<Button3D>();
    [SerializeField]
    private List<Button3D> evidenceButtons = new List<Button3D>();
    private Dictionary<EnemyPreset, Button3D> presetLink = new Dictionary<EnemyPreset, Button3D>();

    private List<EvidenceEnum> selectedEvidence = new List<EvidenceEnum>();

    public override void Initialize(UIController parent)
    {
        base.Initialize(parent);

        List<EnemyPreset> presets = GameController.instance.enemyPresets;
        List<EvidenceEnum> evidenceEnums = Enum.GetValues(typeof(EvidenceEnum)).Cast<EvidenceEnum>().ToList(); ;

        // Run through the buttons and set them up according to the game controller's enemy list
        for (int i = 0; i < nameButtons.Count; i++)
        {
            if (presets.Count > i)
            {
                // Set the text of the button to the enemy name
                nameButtons[i].SetText(presets[i].enemyName);
                presetLink.Add(presets[i], nameButtons[i]);

                // Need this to ensure the delegate does not reference the variable i
                EnemyPreset p = presets[i];

                // Set the button to call the click method from the desired enemy
                nameButtons[i].onClick.AddListener(() => NameButtonClicked(p));
            }
            else
                nameButtons[i].gameObject.SetActive(false);
        }

        // Run through the evidence and set up the delegates on the buttons since Unity inspector can't do it
        for(int i = 0; i < evidenceButtons.Count; i++)
        {
            if (evidenceEnums.Count > i)
            {
                evidenceButtons[i].SetText(evidenceEnums[i].ToString());
                EvidenceEnum e = evidenceEnums[i];
                evidenceButtons[i].onClick.AddListener(() => EvidenceButtonClicked(e));
            }
            else
                evidenceButtons[i].gameObject.SetActive(false);
        }
    }

    public void NameButtonClicked(EnemyPreset p)
    {

    }
    public void EvidenceButtonClicked(EvidenceEnum e)
    {
        if (selectedEvidence.Contains(e))
            selectedEvidence.Remove(e);
        else
            selectedEvidence.Add(e);

        string temp = "Selected Evidence:";
        foreach(EvidenceEnum test in selectedEvidence)
        {
            temp += " " + test.ToString();
        }
        Debug.Log(temp);
        CheckNames();
    }
    private void CheckNames()
    {
        foreach(EnemyPreset p in presetLink.Keys)
        {
            Debug.Log(p.enemyName);
            if (p.CheckEvidence(selectedEvidence))
                presetLink[p].gameObject.SetActive(true);
            else
                presetLink[p].gameObject.SetActive(false);
        }
    }
}
