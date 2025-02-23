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

    public virtual void Initialize(int level = 1) 
    {
        startingLevel = level;
        currentLevel = startingLevel;
    }

    public abstract void Update(float dt);

    public virtual void DetectSound(Vector3 pos, float radius)
    {

    }
    protected virtual void OnLevelChange(int level)
    {
        currentLevel = startingLevel + level - 1;
        currentLevel = currentLevel > maxLevel ? maxLevel : currentLevel;
    }
    public virtual void OnDestroy()
    {
        GameController.OnLevelChange -= OnLevelChange;
    }
}
