using BehaviorTree;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class CheckPlayerInSightChase : Node
{
    private Enemy user;

    private float playerSightTime = 1f;
    private float playerSightTimer;

    private float fovRange;
    private float sightAngle;

    public const string SIGHT_KEY = "playerSightBuffer";
    public const string PLAYER_KEY = "playerKnownPosition";

    private Transform currentTarget;

    public CheckPlayerInSightChase(Enemy user, float fovRange, float sightAngle)
    {
        this.user = user;

        this.fovRange = fovRange;
        this.sightAngle = sightAngle;

        playerSightTimer = playerSightTime;
    }

    public override Status Check(float dt)
    {
        if (GetData(SIGHT_KEY) == null)
            parent.parent.SetData(SIGHT_KEY, false);

        bool playerSightBuffer = (bool)GetData(SIGHT_KEY);

        // Decrement the timer for the player sight buffer
        if (playerSightBuffer)
        {
            playerSightTimer -= Time.deltaTime;

            // If the timer ends, stop the detection of the player
            if (playerSightTimer <= 0)
                SetPlayerSightBuffer(false);
        }

        // Check if the player is close enough to the user
        PriorityQueue<Transform> queue = new PriorityQueue<Transform>();

        // Eliminate the players that are too far from the scan range and place them in closest to furtherst order
        foreach(PlayerController p in PlayerController.playerInstances)
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

                        SetPlayerSightBuffer(true);
                        playerSightTimer = playerSightTime;

                        SetPlayerPosition();

                        status = Status.SUCCESS;
                        return status;
                    }
                }
            }
        }

        // Check if the enemy can still see player due to the buffer
        if (playerSightBuffer)
        {
            SetPlayerPosition();

            status = Status.SUCCESS;
            return status;
        }

        // Check if there is still a known position
        if(GetData(PLAYER_KEY) != null)
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
        parent.parent.SetData(PLAYER_KEY, currentTarget.position);
    }
    public void SetPlayerSightBuffer(bool b)
    {
        parent.parent.SetData(SIGHT_KEY, b);
    }

}
