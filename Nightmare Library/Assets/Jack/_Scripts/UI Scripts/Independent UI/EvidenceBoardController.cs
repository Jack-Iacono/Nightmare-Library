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

public class EvidenceBoardController : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> noteObjects = new List<GameObject>();
    private List<EnemyNoteLink> enemyNotes = new List<EnemyNoteLink>();
    [SerializeField]
    private List<Button3D> evidenceButtons = new List<Button3D>();

    private BiDict<Button3D, EvidenceEnum> evidenceLink = new BiDict<Button3D, EvidenceEnum>();

    private int currentIndex = 0;
    private List<EvidenceScreenData> screenData = new List<EvidenceScreenData>();

    private void Awake()
    {
        if(!GameController.gameStarted)
            GameController.OnGameStart += OnGameStart;
        else
            OnGameStart();

        foreach(GameObject g in noteObjects)
        {
            enemyNotes.Add(new EnemyNoteLink(g));
        }
    }

    private void OnGameStart()
    {
        List<EnemyPreset> presets = PersistentDataController.Instance.enemyPresets;
        List<EvidenceEnum> evidenceEnums = Enum.GetValues(typeof(EvidenceEnum)).Cast<EvidenceEnum>().ToList(); ;

        // Add the spots for the various guesses
        for (int i = 0; i < GameController.enemyCount; i++)
        {
            screenData.Add(new EvidenceScreenData());
        }

        // Run through the buttons and set them up according to the game controller's enemy list
        for (int i = 0; i < enemyNotes.Count; i++)
        {
            if (presets.Count > i)
            {
                // Set the text of the button to the enemy name
                enemyNotes[i].SetEnemy(presets[i]);
            }
            else
                enemyNotes[i].gameObject.SetActive(false);
        }

        // Run through the evidence and set up the delegates on the buttons since Unity inspector can't do it
        for (int i = 0; i < evidenceButtons.Count; i++)
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

        //UpdateCurrentScreen();

        GameController.OnGameStart -= OnGameStart;
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

        CheckNames();
    }

    private void CheckNames()
    {
        foreach(EnemyNoteLink n in enemyNotes)
        {
            if (n.preset.CheckEvidence(screenData[currentIndex].selectedEvidence))
                n.gameObject.SetActive(true);
            else
                n.gameObject.SetActive(false);
        }
    }

    private class EvidenceScreenData
    {
        public List<EvidenceEnum> selectedEvidence = new List<EvidenceEnum>();
        public EnemyPreset guess;
    }

    [Serializable]
    private class EnemyNoteLink
    {
        public GameObject gameObject;
        [NonSerialized] public Image image;
        [NonSerialized] public TMP_Text text;

        [NonSerialized] public EnemyPreset preset;

        public EnemyNoteLink(GameObject gameObject)
        {
            this.gameObject = gameObject;
            image = gameObject.GetComponent<Image>();
            text = gameObject.GetComponentInChildren<TMP_Text>();
        }
        public void SetEnemy(EnemyPreset p)
        {
            preset = p;
            text.text = p.name;
        }
    }
}
