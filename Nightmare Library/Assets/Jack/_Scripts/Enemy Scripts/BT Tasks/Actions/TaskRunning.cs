using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class TaskRunning : Node
{

    public override Status Check(float dt)
    {
        return Status.RUNNING;
    }
}
