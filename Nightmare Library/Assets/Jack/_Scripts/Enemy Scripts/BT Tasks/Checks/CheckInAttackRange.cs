using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class CheckInAttackRange : Node
{
    private float attackRange;
    private Transform transform;
    private Enemy user;

    public const string ATTACK_TARGET_KEY = "currentAttackTarget";
    public const string ATTACKING_KEY = "isCurrentlyAttacking";
    
    public CheckInAttackRange(Enemy user)
    {
        this.user = user;
        transform = user.transform;
        this.attackRange = user.attackRange;
    }

    public override Status Check(float dt)
    {
        Vector3 target = transform.position;

        // If the player is currently attacking, bypass this check
        if (GetData(ATTACKING_KEY) != null && (bool)GetData(ATTACKING_KEY))
        {
            status = Status.SUCCESS;
            return status;
        }

        // Check if the player is in range
        PlayerController p = GetPlayerInRange();
        if (p != null)
        {
            SetRootData(ATTACK_TARGET_KEY, p);
            SetRootData(ATTACKING_KEY, true);

            status = Status.SUCCESS;
            return status;
        }

        ClearData(ATTACKING_KEY);
        ClearData(ATTACK_TARGET_KEY);

        // If the enemy can't see the player and there is no known last position, then it is  a failure
        status = Status.FAILURE;
        return status;
    }

    private PlayerController GetPlayerInRange()
    {
        // Check if the player is close enough to the user
        PriorityQueue<Transform> queue = new PriorityQueue<Transform>();

        // Eliminate the players that are too far from the scan range and place them in closest to furtherst order
        foreach (PlayerController p in PlayerController.playerInstances)
        {
            float dist = Vector3.Distance(p.transform.position, user.transform.position);

            if (dist <= attackRange)
            {
                queue.Insert(new PriorityQueue<Transform>.Element(p.transform, (int)dist));
            }
        }

        while (!queue.Is_Empty())
        {
            Transform player = queue.Extract();

            RaycastHit hit;
            Ray ray = new Ray(user.transform.position, (player.position - user.transform.position).normalized);

            // Check if the player is behind any walls / obstructions
            if (Physics.Raycast(ray.origin, ray.direction, out hit, attackRange))
            {
                if (hit.collider.tag == "Player")
                {
                    return player.gameObject.GetComponent<PlayerController>();
                }
            }
        }

        return null;
    }

}
