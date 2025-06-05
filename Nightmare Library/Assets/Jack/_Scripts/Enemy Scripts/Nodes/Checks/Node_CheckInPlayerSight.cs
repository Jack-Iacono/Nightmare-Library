using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Node_CheckInPlayerSight : Node
{
    private Transform transform;
    private aa_Stalk owner;

    private Enemy user;

    private float fovRange;
    private float sightAngle;
    
    // Override the conditions if already met once
    private bool passCheck = false;

    public Node_CheckInPlayerSight(aa_Stalk owner, Enemy user, float fovRange = 20, float sightAngle = 0.8f)
    {
        this.owner = owner;
        this.fovRange = fovRange;
        this.sightAngle = sightAngle;
        this.user = user;
        transform = user.transform;
    }
    public override Status Check(float dt)
    {
        // Bypass the rest
        if (passCheck)
        {
            status = Status.SUCCESS;
            return status;
        }
        else if(owner.currentTargetDynamic != null)
        {
            foreach(PlayerController p in PlayerController.playerInstances.Values)
            {
                if (!p.isAlive)
                    continue;

                Transform player = p.transform;

                RaycastHit hit;
                Ray ray = new Ray(player.transform.position, (transform.position - player.position).normalized);

                // Check if the player is within the vision arc
                if (Vector3.Dot(player.forward, ray.direction) >= sightAngle)
                {
                    // Check if the player is behind any walls / obstructions
                    if (Physics.Raycast(ray.origin, ray.direction, out hit, fovRange))
                    {
                        if (hit.collider.gameObject == user.gameObject)
                        {
                            passCheck = true;

                            status = Status.SUCCESS;
                            return status;
                        }
                    }
                }
            }
        }

        status = Status.FAILURE;
        return status;
    }

    protected override void OnResetNode()
    {
        base.OnResetNode();
        passCheck = false;
    }
}
