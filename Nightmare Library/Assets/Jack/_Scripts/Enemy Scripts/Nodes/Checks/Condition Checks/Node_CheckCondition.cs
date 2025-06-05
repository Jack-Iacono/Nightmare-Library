using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class Node_CheckCondition : Node
{
    protected bool hasReset = true;

    public Node_CheckCondition()
    {
        
    }
    public override Status Check(float dt)
    {
        if (EvaluateCondition())
        {
            if (!hasReset)
            {
                OnPass();
                hasReset = false;
            }
            else
                OnPassTick();

            status = Status.SUCCESS;
            return status;
        }

        status = Status.FAILURE;
        return status;
    }

    protected abstract bool EvaluateCondition();

    /// <summary>
    /// Runs on the first time the condition is met
    /// </summary>
    protected virtual void OnPass() { }
    /// <summary>
    /// Runs on every tick that the condition is met after the first
    /// </summary>
    protected virtual void OnPassTick() { }

    protected override void OnResetNode()
    {
        base.OnResetNode();
        hasReset = true;
    }
}
