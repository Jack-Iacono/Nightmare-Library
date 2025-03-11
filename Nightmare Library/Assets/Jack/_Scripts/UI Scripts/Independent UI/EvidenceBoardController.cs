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
    private List<Button3D> enemyIndexObjects = new List<Button3D>();

    [SerializeField]
    private List<Button3D> evidenceButtons = new List<Button3D>();

    private BiDict<Button3D, EvidenceEnum> evidenceLink = new BiDict<Button3D, EvidenceEnum>();

    private int currentIndex = 0;
    private EvidenceData[] evidenceData = new EvidenceData[0];

    public delegate void OnEvidenceDataChangeDelegate(int index, EvidenceData data);
    public event OnEvidenceDataChangeDelegate OnEvidenceDataChange;

    private void Awake()
    {
        if(!GameController.gameStarted)
            GameController.OnGameStart += OnGameStart;
        else
            OnGameStart();

        // Add the spots for the various presets
        evidenceData = new EvidenceData[GameController.enemyCount];
        for (int i = 0; i < evidenceData.Length; i++)
        {
            evidenceData[i] = new EvidenceData();
        }
    }

    private void OnGameStart()
    {
        List<EnemyPreset> presets = PersistentDataController.Instance.enemyPresets;
        List<EvidenceEnum> evidenceEnums = Enum.GetValues(typeof(EvidenceEnum)).Cast<EvidenceEnum>().ToList(); ;

        // Goes through all the note gameobjects and decides which are going to be used
        for (int i = 0; i < noteObjects.Count; i++)
        {
            if (i < GameController.enemyCount)
                enemyNotes.Add(new EnemyNoteLink(noteObjects[i]));
            else
                noteObjects[i].SetActive(false);
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

        // This deactivates any index markers that won't be used to represent enemy indexes. This is used when altering the enemy count
        if(GameController.enemyCount < enemyIndexObjects.Count)
        {
            for (int i = enemyIndexObjects.Count - 1; i > GameController.enemyCount - 1; i--)
            {
                enemyIndexObjects[i].gameObject.SetActive(false);
                enemyIndexObjects.RemoveAt(i);
            }
        }

        UpdateEnemyIndexObjects();
        UpdateEvidenceButtons();

        GameController.OnGameStart -= OnGameStart;
    }

    public void EvidenceButtonClick(Interactable interactable, bool fromNetwork)
    {
        Button3D button = (Button3D)interactable;
        int eIndex = (int)evidenceLink[button];

        // Toggle the evidence on / off
        evidenceData[currentIndex].evidence[eIndex] = !evidenceData[currentIndex].evidence[eIndex];

        OnEvidenceDataChange?.Invoke(currentIndex, evidenceData[currentIndex]);

        UpdateEvidenceButtons();
    }
    public void EnemyIndexButtonClick(int index)
    {
        currentIndex = index;
        UpdateEnemyIndexObjects();
    }

    public void UpdateEnemyIndexObjects()
    {
        // This sets the current index object to not be seen to indicate that it is in use
        for (int i = 0; i < enemyIndexObjects.Count; i++)
        {
            if (i == currentIndex)
                enemyIndexObjects[i].gameObject.SetActive(false);
            else
                enemyIndexObjects[i].gameObject.SetActive(true);
        }

        UpdateEnemyNames();
    }
    public void UpdateEvidenceButtons()
    {
        // Run through the evidence buttons and set the screen up to look like the data
        foreach (Button3D button in evidenceLink.Keys1)
        {
            // Run through each evidenceData for that button
            for (int i = 0; i < evidenceData.Length; i++)
            {
                // check if that evidence is being selected
                if (evidenceData[i].evidence[(int)evidenceLink[button]])
                {
                    button.SetColor(Color.grey);
                }
                else
                {
                    button.SetColor(Color.white);
                }
            }
        }

        UpdateEnemyNames();
    }
    private void UpdateEnemyNames()
    {
        foreach(EnemyNoteLink n in enemyNotes)
        {
            if (n.preset.CheckEvidence(evidenceData[currentIndex].evidence))
                n.gameObject.SetActive(true);
            else
                n.gameObject.SetActive(false);
        }
    }
    
    public void SetEvidenceData(bool[] data)
    {
        // Add the spots for the various presets
        for (int i = 0; i < data.Length; i++)
        {
            evidenceData[Mathf.FloorToInt(i / EvidenceTypeCount)].evidence[i % EvidenceTypeCount] = data[i];
        }
        UpdateEvidenceButtons();
    }

    public class EvidenceData
    {
        public bool[] evidence = new bool[0];
        public EvidenceData()
        {
            // Creates an array to hold to on or off values of each of the evidence types present
            evidence = new bool[Enum.GetValues(typeof(EvidenceEnum)).Length];
        }
        public EvidenceData(bool[] evidence)
        {
            this.evidence = evidence;
        }
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
