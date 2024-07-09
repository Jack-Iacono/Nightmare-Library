using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class CheckRushStop : Node
{
    Enemy user;

    private float wallCheckDistance = 5;

    // Not sure if this is right, check later
    private LayerMask wallLayers = 1;

    public CheckRushStop(Enemy user)
    {
        this.user = user;
    }

    public override Status Check(float dt)
    {
        object temp = GetData(TaskStartRush.RUSH_KEY);
        bool isRushing = false;

        if (temp != null)
            isRushing = (bool)temp;

        if (isRushing)
        {
            Debug.Log("Is Rushing");

            Ray wallRay = new Ray(user.transform.position, user.transform.forward);

            // Check for the ray hitting a wall
            if(Physics.Raycast(wallRay, wallCheckDistance, wallLayers))
            {
                Debug.Log("Stopping Rush");

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
