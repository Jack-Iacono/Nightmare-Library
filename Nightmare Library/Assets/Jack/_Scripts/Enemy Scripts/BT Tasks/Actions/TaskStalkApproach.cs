 using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TaskStalkApproach : Node
{
    private aa_Stalk owner;
    private NavMeshAgent navAgent;

    private float speed;
    private float acceleration;

    public TaskStalkApproach(aa_Stalk owner, NavMeshAgent navAgent, float speed = 5, float acceleration = 400)
    {
        this.owner = owner;
        this.navAgent = navAgent;
        this.speed = speed;
        this.acceleration = acceleration;
    }
    public override Status Check(float dt)
    {
        if(owner.stalkAttemptCounter > 0)
        {
            navAgent.destination = owner.currentTarget.position;
            navAgent.speed = speed;
            navAgent.acceleration = acceleration;

            status = Status.RUNNING;
            return status;
        }

        status = Status.FAILURE;
        return status;
    }
}
