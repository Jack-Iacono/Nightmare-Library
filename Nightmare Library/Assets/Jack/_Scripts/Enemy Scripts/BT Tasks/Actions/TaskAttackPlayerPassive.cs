using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class TaskAttackPlayerPassive : Node
{

    public override Status Check(float dt)
    {
        Debug.Log("Running");
        foreach(PlayerController player in DeskController.playersAtDesk)
        {
            player.ReceiveAttack();
        }

        return Status.SUCCESS;
    }

}
