using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TaskStalkCloseIn : Node
{
    private aa_Stalk owner;
    private NavMeshAgent navAgent;

    private float speed;
    private float acceleration;

    public TaskStalkCloseIn(aa_Stalk owner, NavMeshAgent navAgent, float speed = 20, float acceleration = 400)
    {
        this.owner = owner;
        this.navAgent = navAgent;
        this.speed = speed;
        this.acceleration = acceleration;
    }
    public override Status Check(float dt)
    {
        Debug.Log("Closing In");

        navAgent.destination = owner.currentTargetDynamic.position;
        navAgent.speed = speed;
        navAgent.acceleration = acceleration;

        status = Status.RUNNING;
        return status;
    }

    public void OnLevelChange(float speed, float accerleration)
    {
        this.speed = speed;
        this.acceleration = accerleration;
    }
}
