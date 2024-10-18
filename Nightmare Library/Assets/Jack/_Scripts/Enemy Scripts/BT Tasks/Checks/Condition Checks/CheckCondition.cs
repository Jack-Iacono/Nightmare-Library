using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class CheckCondition : Node
{
    protected bool hasReset = true;

    public CheckCondition()
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

    protected virtual void OnPass() { }
    protected virtual void OnPassTick() { }

    protected override void OnResetNode()
    {
        base.OnResetNode();
        hasReset = true;
    }
}
