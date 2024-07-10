using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class CheckIdolCount : Node
{
    private int maxIdolCount;

    public CheckIdolCount(int maxIdolCount)
    {
        this.maxIdolCount = maxIdolCount;
    }

    public override Status Check(float dt)
    {
        return TaskSpawnIdols.currentIdolCount >= maxIdolCount ? Status.SUCCESS : Status.FAILURE;
    }

}
