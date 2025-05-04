using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class TaskStalkAssignPlayer : Node
{
    private aa_Stalk owner;
    public TaskStalkAssignPlayer(aa_Stalk owner)
    {
        this.owner = owner;
    }
    public override Status Check(float dt)
    {
        return base.Check(dt);
    }
}
