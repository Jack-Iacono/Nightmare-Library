using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class CheckPlayerInSightRush : Node
{
    private Enemy user;

    private float fovRange;
    private float sightAngle;

    public const string PLAYER_KEY = "playerKnownPosition";

    private Transform currentTarget;

    public CheckPlayerInSightRush(Enemy user, float fovRange, float sightAngle)
    {
        this.user = user;

        this.fovRange = fovRange;
        this.sightAngle = sightAngle;
    }

    public override Status Check(float dt)
    {
        // Check if the player is close enough to the user
        PriorityQueue<Transform> queue = new PriorityQueue<Transform>();

        // Eliminate the players that are too far from the scan range and place them in closest to furtherst order
        foreach (PlayerController p in PlayerController.playerInstances)
        {
            float dist = Vector3.Distance(p.transform.position, user.transform.position);

            if (dist <= fovRange)
            {
                queue.Insert(new PriorityQueue<Transform>.Element(p.transform, (int)dist));
            }
        }

        while (!queue.Is_Empty())
        {
            Transform player = queue.Extract();

            RaycastHit hit;
            Ray ray = new Ray(user.transform.position, (player.position - user.transform.position).normalized);

            // Check if the player is within the vision arc
            if (Vector3.Dot(user.transform.forward, ray.direction) >= sightAngle)
            {
                // Check if the player is behind any walls / obstructions
                if (Physics.Raycast(ray.origin, ray.direction, out hit, fovRange))
                {
                    if (hit.collider.tag == "Player")
                    {
                        currentTarget = player;

                        SetPlayerPosition();

                        //user.navAgent.destination = user.transform.position;

                        user.navAgent.speed = 0;

                        status = Status.SUCCESS;
                        return status;
                    }
                }
            }
        }

        // Check if there is still a known position
        if (GetData(PLAYER_KEY) != null)
        {
            status = Status.SUCCESS;
            return status;
        }

        // If the enemy can't see the player and there is no known last position, then it is  a failure
        status = Status.FAILURE;
        return status;
    }

    public void SetPlayerPosition()
    {
        Debug.Log("Update Player Position");
        Ray ray = new Ray(user.transform.position, (currentTarget.position - user.transform.position).normalized);
        RaycastHit hit;

        Physics.Raycast(ray, out hit, 1000, 1);
        Debug.DrawRay(ray.origin, ray.direction * 1000, Color.cyan, 0.1f);

        parent.parent.SetData(PLAYER_KEY, hit.point);
    }
}
