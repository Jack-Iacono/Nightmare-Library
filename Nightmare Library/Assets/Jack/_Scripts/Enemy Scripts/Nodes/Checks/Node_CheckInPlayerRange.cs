using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class Node_CheckInPlayerRange : Node
{
    private float range;
    private Transform transform;
    private Enemy user;

    public Node_CheckInPlayerRange(Enemy user, float range)
    {
        this.user = user;
        transform = user.transform;
        this.range = range;
    }

    public override Status Check(float dt)
    {
        // Checks if any player is in attack range
        foreach (PlayerController p in PlayerController.playerInstances.Values)
        {
            if(p.isAlive && Vector3.Distance(p.transform.position, transform.position) < range)
            {
                status = Status.SUCCESS;
                return status;
            }
        }

        // If the enemy can't see the player and there is no known last position, then it is  a failure
        status = Status.FAILURE;
        return status;
    }
}
