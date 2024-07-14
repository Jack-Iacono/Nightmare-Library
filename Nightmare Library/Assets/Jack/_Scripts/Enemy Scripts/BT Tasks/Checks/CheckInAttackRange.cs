using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class CheckInAttackRange : Node
{
    private float attackRange;
    private float sightAngle;
    private Transform transform;
    
    public CheckInAttackRange(Transform attacker, float attackRange, float sightAngle)
    {
        transform = attacker;
        this.attackRange = attackRange;
        this.sightAngle = sightAngle;
    }

    public override Status Check(float dt)
    {
        Vector3 target = transform.position;

        // If the player is currently attacking, bypass this check
        if (GetData("attacking") != null && (bool)GetData("attacking"))
        {
            status = Status.SUCCESS;
            return status;
        }

        // Check if the player is close enough to the user
        if (Vector3.Distance(target, transform.position) <= attackRange)
        {
            // Check if the player is within the vision arc
            if (Vector3.Dot(transform.forward, (target - transform.position).normalized) >= sightAngle)
            {
                status = Status.SUCCESS;
                return status;
            }
        }

        ClearData("attacking");

        // If the enemy can't see the player and there is no known last position, then it is  a failure
        status = Status.FAILURE;
        return status;
    }

}
