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

    private bool seenPlayer;

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
                    if (hit.collider.tag == "Player")
                    {
                        SetPlayerPosition();
                        navAgent.speed = 0;
                        seenPlayer = true;
                    }
                }
            }
        }

        // Check if there is still a known position
        if (seenPlayer)
        {
            status = Status.SUCCESS;
            return status;
        }

        seenPlayer = false;

        // If the enemy can't see the player and there is no known last position, then it is  a failure
        status = Status.FAILURE;
        return status;
    }

    public void SetPlayerPosition()
    {
        Ray ray = new Ray(transform.position, (owner.currentTargetDynamic.position - transform.position).normalized);
        RaycastHit hit;

        Physics.Raycast(ray, out hit, 1000, aa_RushOutdated.envLayers);
        Debug.DrawRay(ray.origin, ray.direction * 1000, Color.cyan, 0.1f);

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(transform.rotation.x, Mathf.Atan2(ray.direction.x, ray.direction.z) * Mathf.Rad2Deg, transform.rotation.z), 0.05f);

        owner.SetCurrentTarget(hit.point);
    }
}
