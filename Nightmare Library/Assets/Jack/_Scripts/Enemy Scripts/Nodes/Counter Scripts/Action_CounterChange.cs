using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class Action_CounterChange : Node
{
    private Check_Counter counter;
    private int value;

    public enum ChangeType { ADD, SUBTRACT, SET, RESET }
    private ChangeType changeType;

    public Action_CounterChange(Check_Counter counter, ChangeType changeType, int value = 0)
    {
        this.counter = counter;
        this.value = value;
        this.changeType = changeType;
    }

    public override Status Check(float dt)
    {
        switch (changeType)
        {
            case ChangeType.ADD:
                counter.Increment(value);
                break;
            case ChangeType.SUBTRACT:
                counter.Decrement(value);
                break;
            case ChangeType.SET:
                counter.Set(value);
                break;
            case ChangeType.RESET:
                counter.Reset();
                break;
        }

        return Status.SUCCESS;
    }
}
