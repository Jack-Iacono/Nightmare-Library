using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PassiveAttack
{
    public string name;
    public string toolTip;

    protected Enemy owner;

    public PassiveAttack(Enemy owner)
    {
        this.owner = owner;
    }

    public virtual void Initialize() { }
    public abstract void Update(float dt);

    public virtual void OnDestroy()
    {

    }
}
