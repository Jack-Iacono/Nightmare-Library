using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class CheckIsRushing : Node
{
    public override Status Check(float dt)
    {
        object temp = GetData(TaskRushTarget.RUSH_KEY);
        bool isRushing = false;

        if (temp != null)
            isRushing = (bool)temp;

        if (isRushing)
        {
            status = Status.SUCCESS;
            return status;
        }

        status = Status.FAILURE;
        return status;
    }
}
