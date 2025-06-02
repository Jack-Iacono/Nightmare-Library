using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class Action_DebugMessage : Node
{
    private string displayMessage = "Debug Message Node";

    public Action_DebugMessage(string displayMessage)
    {
        this.displayMessage = displayMessage;
    }

    public override Status Check(float dt)
    {
        Debug.Log(displayMessage);
        return Status.SUCCESS;
    }
}
