using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class CheckArea : Node
{
    private float waitTime = 1f;
    private float waitTimer = 1f;

    private bool waiting = false;

    private Transform player;
    private Enemy user;

    public CheckArea(Enemy user, Transform player)
    {
        this.player = player;
        this.user = user;
    }

    public override Status Check(float dt)
    {
        if (waiting)
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer > 0)
            {
                status = Status.RUNNING;
                return status;
            }

            waiting = false;

            ClearData("playerKnownPosition");

            status = Status.SUCCESS;
            return status;
        }
        else
        {
            // Check if the player is close enough to the user
            if (Vector3.Distance(player.position, user.transform.position) <= user.fovRange)
            {
                RaycastHit hit;
                Ray ray = new Ray(user.transform.position, (player.position - user.transform.position).normalized);

                Debug.Log(Vector3.Dot(user.transform.forward, ray.direction));

                // Check if the player is within the vision arc
                if (Vector3.Dot(user.transform.forward, ray.direction) >= 0.1)
                {
                    // Check if the player is behind any walls / obstructions
                    if (Physics.Raycast(ray.origin, ray.direction, out hit, user.fovRange))
                    {
                        if (hit.collider.tag == "Player")
                        {
                            waiting = false;
                            waitTimer = waitTime;

                            parent.SetData("playerKnownPosition", player.position);

                            status = Status.SUCCESS;
                            return status;
                        }
                    }
                }
            }

            waitTimer = waitTime;
            waiting = true;

            status = Status.RUNNING;
            return status;
        }
    }
}
