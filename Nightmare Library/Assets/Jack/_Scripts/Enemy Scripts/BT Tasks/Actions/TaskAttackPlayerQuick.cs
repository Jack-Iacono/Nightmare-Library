using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;
using UnityEngine.AI;

public class TaskAttackPlayerQuick : TaskWait
{
    public TaskAttackPlayerQuick(float waitTimer, Enemy owner) : base(waitTimer)
    {
    }

    protected override void OnStart()
    {
        PlayerController player = (PlayerController)GetData(CheckInAttackRange.ATTACK_TARGET_KEY);
        if(player != null)
        {
            player.ReceiveAttack();
        }

    }
    protected override void OnEnd()
    {
        ClearData(CheckInAttackRange.ATTACKING_KEY);
        ClearData(CheckInAttackRange.ATTACK_TARGET_KEY);
    }
    protected override void OnTick(float time)
    {

    }
}
