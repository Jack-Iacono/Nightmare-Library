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

    private BiDict<Button3D, EnemyPreset> presetLink = new BiDict<Button3D, EnemyPreset>();
    private BiDict<Button3D, EvidenceEnum> evidenceLink = new BiDict<Button3D, EvidenceEnum>();

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
                presetLink.Add(nameButtons[i], presets[i]);
                nameButtons[i].OnClick += NameButtonClicked;
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
                evidenceLink.Add(evidenceButtons[i], evidenceEnums[i]);
                evidenceButtons[i].OnClick += EvidenceButtonClicked;
            }
            else
                evidenceButtons[i].gameObject.SetActive(false);
        }
    }

    public void NameButtonClicked(Interactable interactable, bool fromNetwork)
    {
        Debug.Log(presetLink[(Button3D)interactable].name);
    }
    public void EvidenceButtonClicked(Interactable interactable, bool fromNetwork)
    {
        Button3D button = (Button3D)interactable;
        EvidenceEnum e = evidenceLink[button];

        if (selectedEvidence.Contains(e))
        {
            selectedEvidence.Remove(e);
            button.SetColor(Color.white);
        }
        else
        {
            selectedEvidence.Add(e);
            button.SetColor(Color.grey);
        }
            

        CheckNames();
    }

    private void CheckNames()
    {
        foreach(EnemyPreset p in presetLink.Keys2)
        {
            if (p.CheckEvidence(selectedEvidence))
                presetLink[p].gameObject.SetActive(true);
            else
                presetLink[p].gameObject.SetActive(false);
        }
    }

}
