using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class Node_StalkAssignPlayer : Node
{
    private aa_Stalk owner;
    public Node_StalkAssignPlayer(aa_Stalk owner)
    {
        this.owner = owner;
    }
    public override Status Check(float dt)
    {
        return base.Check(dt);
    }
}
