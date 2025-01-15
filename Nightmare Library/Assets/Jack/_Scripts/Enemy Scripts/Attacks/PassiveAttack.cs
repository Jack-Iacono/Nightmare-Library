using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PassiveAttack
{
    public string name;
    public string toolTip;

    protected Enemy owner;

    protected int startingLevel = 1;
    public int currentLevel { get; protected set; }
    public const int maxLevel = 10;

    public PassiveAttack(Enemy owner)
    {
        this.owner = owner;
        GameController.OnLevelChange += OnLevelChange;
        currentLevel = startingLevel;
    }

    public virtual void Initialize() { }
    public abstract void Update(float dt);

    public Enemy GetOwner()
    {
        return owner;
    }

    protected void OnLevelChange(int level)
    {
        currentLevel = startingLevel + level - 1;
        currentLevel = currentLevel > maxLevel ? maxLevel : currentLevel;
    }

    public virtual void OnDestroy()
    {
        GameController.OnLevelChange -= OnLevelChange;
    }
}
