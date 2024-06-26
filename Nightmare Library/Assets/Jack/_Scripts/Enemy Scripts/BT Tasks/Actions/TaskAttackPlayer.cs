using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;
using UnityEngine.AI;

public class TaskAttackPlayer : TaskWait
{
    private PlayerController player;
    BasicEnemy owner;
    NavMeshAgent navAgent;

    private bool hasAttacked = false;

    public TaskAttackPlayer(string waitLabel, float waitTimer, PlayerController player, BasicEnemy owner) : base(waitLabel, waitTimer)
    {
        this.owner = owner;
        navAgent = owner.navAgent;
        this.player = player;
    }

    protected override void OnStart()
    {
        hasAttacked = false;

        navAgent.destination = navAgent.transform.position;
    }
    protected override void OnEnd()
    {
        hasAttacked = false;
    }
    protected override void OnTick(float time)
    {
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
    }
}
