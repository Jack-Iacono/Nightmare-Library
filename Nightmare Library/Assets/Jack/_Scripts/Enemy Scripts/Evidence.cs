using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Evidence
{
    public string name;
    public string toolTip;
    protected Enemy owner;

    public Evidence(Enemy owner)
    {
        this.owner = owner;
    }

    public abstract void UpdateProcess(float dt);
}
