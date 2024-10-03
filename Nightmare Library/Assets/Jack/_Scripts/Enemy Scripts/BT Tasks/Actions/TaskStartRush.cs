using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;
using UnityEngine.AI;

public class TaskStartRush : Node
{
    

    Enemy owner;

    public TaskStartRush(Enemy owner) 
    {
        this.owner = owner;
    }

    public override Status Check(float dt)
    {
        // Set up the enemy for the rush
        parent.parent.SetData(TaskRushTarget.RUSH_KEY, true);

        status = Status.SUCCESS;
        return status;
    }
}
