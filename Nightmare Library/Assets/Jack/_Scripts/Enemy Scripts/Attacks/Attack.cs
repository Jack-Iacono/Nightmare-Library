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

    protected float hearingRadius = -1;
    protected List<AudioSourceController.SourceData> recentAudioSources = new List<AudioSourceController.SourceData>();

    public virtual void Initialize(int level = 1) 
    {
        startingLevel = level;
        currentLevel = startingLevel;
    }

    public abstract void Update(float dt);

    public virtual bool DetectSound(AudioSourceController.SourceData data)
    {
        if(hearingRadius == -1)
            return false;

        // Check if the sound's radius and the hearing radius overlap, allowing the enemy to hear the sound
        if(Vector3.Distance(data.transform.position, owner.transform.position) <= hearingRadius + data.radius)
        {
            // Could add raycast here to detratc from volume if heard through wall
            Debug.Log("Detect Sound");
            return true;
        }

        return false;
    }
    protected virtual void OnLevelChange(int level)
    {
        currentLevel = startingLevel + level - 1;
        currentLevel = currentLevel > maxLevel ? maxLevel : currentLevel;
    }

    public void RemoveAudioSource(int index)
    {
        recentAudioSources.RemoveAt(index);
    }
    public void RemoveAudioSource(AudioSourceController.SourceData data)
    {
        recentAudioSources.Remove(data);
    }
    public AudioSourceController.SourceData GetAudioSource(int index)
    {
        if(recentAudioSources.Count <= index) return null;
        return recentAudioSources[index];
    }

    public virtual void OnDestroy()
    {
        GameController.OnLevelChange -= OnLevelChange;
    }
}
