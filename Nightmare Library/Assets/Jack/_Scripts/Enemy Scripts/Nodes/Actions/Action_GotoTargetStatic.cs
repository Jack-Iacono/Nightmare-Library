using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;
using UnityEngine.AI;

public class Action_GotoTargetStatic : Node
{
    private NavMeshAgent agent;
    private Transform transform;
    private ActiveAttack owner;

    private float speed;
    private float acceleration;

    // Passes the check if the conditions have been met previously, this saves processing power
    private bool passCheck;

    public Action_GotoTargetStatic(ActiveAttack owner, Enemy enemy, float speed = 8, float acceleration = 300)
    {
        agent = enemy.navAgent;
        transform = enemy.transform;

        this.owner = owner;

        this.speed = speed;
        this.acceleration = acceleration;
    }
    public override Status Check(float dt)
    {
        if (passCheck)
        {
            status = Status.SUCCESS;
            return status;
        }

        agent.destination = owner.currentTargetStatic;
        agent.speed = speed;
        agent.acceleration = 1000;

        // Is the player close enough to the target node
        if (Vector3.SqrMagnitude(transform.position - owner.currentTargetStatic) < 1)
        {
            passCheck = true;

            status = Status.SUCCESS;
            return status;
        }

        status = Status.RUNNING;
        return status;
    }

    protected override void OnResetNode()
    {
        passCheck = false;
        base.OnResetNode();
    }

    public void OnLevelChange(float speed, float acceleration)
    {
        this.speed = speed;
        this.acceleration = acceleration;
    }
}
