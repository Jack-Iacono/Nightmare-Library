using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;
using UnityEngine.AI;

public class TaskAttackPlayerQuick : TaskWait
{
    Enemy owner;
    NavMeshAgent navAgent;
    Transform currentTarget;

    //private bool hasAttacked = false;

    public TaskAttackPlayerQuick(string waitLabel, float waitTimer, Enemy owner) : base(waitLabel, waitTimer)
    {
        this.owner = owner;
        navAgent = owner.navAgent;
    }

    protected override void OnStart()
    {
        //hasAttacked = false;

        PlayerController player = (PlayerController)GetData(CheckInAttackRange.ATTACK_TARGET_KEY);
        if(player != null)
        {
            player.AttackPlayer();
        }

    }
    protected override void OnEnd()
    {
        //hasAttacked = false;

        ClearData(CheckInAttackRange.ATTACKING_KEY);
        ClearData(CheckInAttackRange.ATTACK_TARGET_KEY);
    }
    protected override void OnTick(float time)
    {
        /*
        if (!hasAttacked && time < 0.6)
        {
            // Check if the player is still close enough to the user
            if (Vector3.Distance(player.transform.position, owner.transform.position) <= 2)
            {
                // Check if the player is within the vision arc
                if (Vector3.Dot(owner.transform.forward, (player.transform.position - owner.transform.position).normalized) >= 0.2f)
                {
                    hasAttacked = true;
                }
            }
        }
        */
    }
}
