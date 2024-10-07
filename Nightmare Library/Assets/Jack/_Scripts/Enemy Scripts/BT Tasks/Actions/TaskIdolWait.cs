using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskIdolWait : TaskWait
{
    private string placementLabel;

    public TaskIdolWait(string placementLabel, float waitTime, float waitDiff = 0) : base(waitTime, waitDiff)
    {
        this.placementLabel = placementLabel;
    }

    protected override void OnStart() { }
    protected override void OnEnd() 
    {
        parent.SetData(placementLabel, true);
    }
    protected override void OnTick(float time) { }
}
