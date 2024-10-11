using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using BehaviorTree;

public class TaskMakeNoise : Node
{
    private bool makeNoise = true;

    public TaskMakeNoise()
    {
        
    }
    public override Status Check(float dt)
    {
        if (makeNoise)
        {
            Debug.Log("Make Noise");
            makeNoise = false;
        }

        status = Status.SUCCESS;
        return status;
    }
    protected override void OnResetNode()
    {
        base.OnResetNode();
        makeNoise = false;
    }
}
