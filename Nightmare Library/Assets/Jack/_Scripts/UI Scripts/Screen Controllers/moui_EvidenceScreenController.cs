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
    private List<Button3D> presetButtons = new List<Button3D>();
    [SerializeField]
    private List<Button3D> evidenceButtons = new List<Button3D>();
    [SerializeField]
    private Button3D nameButton;

    private BiDict<Button3D, EnemyPreset> presetLink = new BiDict<Button3D, EnemyPreset>();
    private BiDict<Button3D, EvidenceEnum> evidenceLink = new BiDict<Button3D, EvidenceEnum>();

    private int currentIndex = 0;
    private List<EvidenceScreenData> screenData = new List<EvidenceScreenData>();

    public override void Initialize(UIController parent)
    {
        base.Initialize(parent);

        List<EnemyPreset> presets = GameController.instance.enemyPresets;
        List<EvidenceEnum> evidenceEnums = Enum.GetValues(typeof(EvidenceEnum)).Cast<EvidenceEnum>().ToList(); ;

        // Add the spots for the various guesses
        for(int i = 0; i < GameController.enemyCount; i++)
        {
            screenData.Add(new EvidenceScreenData());
        }

        // Run through the buttons and set them up according to the game controller's enemy list
        for (int i = 0; i < presetButtons.Count; i++)
        {
            if (presets.Count > i)
            {
                // Set the text of the button to the enemy name
                presetButtons[i].SetText(presets[i].enemyName);
                presetLink.Add(presetButtons[i], presets[i]);
                presetButtons[i].OnClick += PresetButtonClick;
            }
            else
                presetButtons[i].gameObject.SetActive(false);
        }

        // Run through the evidence and set up the delegates on the buttons since Unity inspector can't do it
        for(int i = 0; i < evidenceButtons.Count; i++)
        {
            if (evidenceEnums.Count > i)
            {
                evidenceButtons[i].SetText(evidenceEnums[i].ToString());
                evidenceLink.Add(evidenceButtons[i], evidenceEnums[i]);
                evidenceButtons[i].OnClick += EvidenceButtonClick;
            }
            else
                evidenceButtons[i].gameObject.SetActive(false);
        }

        nameButton.OnClick += NameButtonClick;

        UpdateCurrentScreen();
    }

    public void PresetButtonClick(Interactable interactable, bool fromNetwork)
    {
        GameController.MakeGuess(currentIndex, presetLink[(Button3D)interactable]);

        screenData[currentIndex].guess = presetLink[(Button3D)interactable];
        UpdateCurrentScreen();
    }
    public void EvidenceButtonClick(Interactable interactable, bool fromNetwork)
    {
        Button3D button = (Button3D)interactable;
        EvidenceEnum e = evidenceLink[button];

        if (screenData[currentIndex].selectedEvidence.Contains(e))
            screenData[currentIndex].selectedEvidence.Remove(e);
        else
            screenData[currentIndex].selectedEvidence.Add(e);

        UpdateCurrentScreen();
    }
    public void NameButtonClick(Interactable interactable, bool fromNetwork)
    {
        currentIndex = (currentIndex + 1) % screenData.Count;
        UpdateCurrentScreen();
    }

    private void UpdateCurrentScreen()
    {
        // Run through the evidence buttons and set the screen up to look like the data
        foreach(Button3D button in evidenceLink.Keys1)
        {
            if (screenData[currentIndex].selectedEvidence.Contains(evidenceLink[button]))
            {
                button.SetColor(Color.grey);
            }
            else
            {
                button.SetColor(Color.white);
            }
        }

        // Greys out the selected button
        foreach (Button3D button in presetLink.Keys1)
        {
            if (screenData[currentIndex].guess == presetLink[button])
            {
                button.SetColor(Color.grey);
            }
            else
            {
                button.SetColor(Color.white);
            }
        }

        if (screenData[currentIndex].guess != null)
            nameButton.SetText(screenData[currentIndex].guess.enemyName);
        else
            nameButton.SetText("Creature " + currentIndex);

        CheckNames();
    }

    private void CheckNames()
    {
        foreach(EnemyPreset p in presetLink.Keys2)
        {
            if (p.CheckEvidence(screenData[currentIndex].selectedEvidence))
                presetLink[p].gameObject.SetActive(true);
            else
                presetLink[p].gameObject.SetActive(false);
        }
    }

    private class EvidenceScreenData
    {
        public List<EvidenceEnum> selectedEvidence = new List<EvidenceEnum>();
        public EnemyPreset guess;
    }
}
