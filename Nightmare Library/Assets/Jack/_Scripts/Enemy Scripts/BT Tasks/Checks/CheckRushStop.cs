using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class CheckRushStop : Node
{
    aa_RushOutdated owner;
    Enemy user;

    private float wallCheckDistance = 5;

    public CheckRushStop(Enemy user, aa_RushOutdated owner)
    {
        this.user = user;
    }

    public override Status Check(float dt)
    {
        if (owner.isRushing)
        {
            Ray wallRay = new Ray(user.transform.position, user.transform.forward);

            Debug.DrawRay(wallRay.origin, wallRay.direction, Color.cyan, 0.1f);

            // Check for the ray hitting a wall
            if (Physics.Raycast(wallRay, wallCheckDistance, ActiveAttack.envLayers))
            {
                status = Status.SUCCESS;
                return status;
            }

            status = Status.RUNNING;
            return status;
        }

        status = Status.FAILURE;
        return status;
    }
}
