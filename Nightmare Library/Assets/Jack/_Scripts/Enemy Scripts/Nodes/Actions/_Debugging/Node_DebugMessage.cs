using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class Node_DebugMessage : Node
{
    private string displayMessage = "Debug Message Node";

    public Node_DebugMessage(string displayMessage)
    {
        this.displayMessage = displayMessage;
    }

    public override Status Check(float dt)
    {
        Debug.Log(displayMessage);
        return Status.SUCCESS;
    }
}
