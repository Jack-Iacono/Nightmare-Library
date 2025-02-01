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

    public virtual void Initialize() { }

    public abstract void Update(float dt);

    protected void OnLevelChange(int level)
    {
        Debug.Log("Current Level for " + name + " set to " + level);
        currentLevel = startingLevel + level - 1;
        currentLevel = currentLevel > maxLevel ? maxLevel : currentLevel;
    }
    public virtual void OnDestroy()
    {
        GameController.OnLevelChange -= OnLevelChange;
    }
}
