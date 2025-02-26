using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Attack
{
    public string name;
    public string toolTip;

    protected Enemy owner;

    public const int maxLevel = 10;
    protected int startingLevel = 1;
    public int currentLevel;

    protected List<AudioSourceController.SourceData> recentAudioSources = new List<AudioSourceController.SourceData>();

    public virtual void Initialize(int level = 1) 
    {
        startingLevel = level;
        currentLevel = startingLevel;
    }

    public abstract void Update(float dt);

    public virtual void DetectSound(AudioSourceController.SourceData data)
    {

    }
    protected virtual void OnLevelChange(int level)
    {
        currentLevel = startingLevel + level - 1;
        currentLevel = currentLevel > maxLevel ? maxLevel : currentLevel;
    }

    public void RemoveFirstAudioSource()
    {
        recentAudioSources.RemoveAt(0);
    }
    public AudioSourceController.SourceData GetFirstAudioSource()
    {
        if(recentAudioSources.Count == 0) return null;
        return recentAudioSources[0];
    }

    public virtual void OnDestroy()
    {
        GameController.OnLevelChange -= OnLevelChange;
    }
}
