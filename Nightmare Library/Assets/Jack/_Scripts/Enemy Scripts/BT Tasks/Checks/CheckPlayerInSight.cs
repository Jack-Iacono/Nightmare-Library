using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;
using UnityEngine.AI;

public class CheckPlayerInSight : Node
{
    private ActiveAttack owner;
    private NavMeshAgent navAgent;
    private Transform transform;

    private float fovRange;
    private float sightAngle;

    public CheckPlayerInSight(ActiveAttack owner, NavMeshAgent navAgent, float fovRange, float sightAngle)
    {
        this.owner = owner;
        this.navAgent = navAgent;
        transform = navAgent.transform;

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
            float dist = Vector3.Distance(p.transform.position, transform.position);

            if (dist <= fovRange)
            {
                queue.Insert(new PriorityQueue<Transform>.Element(p.transform, (int)dist));
            }
        }

        // Go through the queue and evaluate all players
        while (!queue.Is_Empty())
        {
            Transform player = queue.Extract();

            RaycastHit hit;
            Ray ray = new Ray(transform.position, (player.position - transform.position).normalized);

            // Check if the player is within the vision arc
            if (Vector3.Dot(transform.forward, ray.direction) >= sightAngle)
            {
                // Check if the player is behind any walls / obstructions
                if (Physics.Raycast(ray.origin, ray.direction, out hit, fovRange))
                {
                    if (hit.collider.transform == player)
                    {
                        SetPlayerPosition(player);
                    }
                }
            }
        }

        // If the enemy can't see the player and there is no known last position, then it is  a failure
        status = Status.FAILURE;
        return status;
    }

    public void SetPlayerPosition(Transform p)
    {
        owner.SetCurrentTarget(p);
    }
}
