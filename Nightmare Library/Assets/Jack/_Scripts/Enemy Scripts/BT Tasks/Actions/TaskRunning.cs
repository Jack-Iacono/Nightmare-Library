using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

/// <summary>
/// This is used to continue looping the current nodes event after the prior would have returned success. 
/// Used to make what is essentially a while loop where the loop needs to run an indefenite amount of times only until the condition is met
/// </summary>
public class TaskRunning : Node
{
    public override Status Check(float dt)
    {
        return Status.RUNNING;
    }
}
