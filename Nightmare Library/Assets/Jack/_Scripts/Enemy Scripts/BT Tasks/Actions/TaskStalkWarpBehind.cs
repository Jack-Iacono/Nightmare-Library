 using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TaskStalkWarpBehind : Node
{
    private aa_Stalk owner;
    private NavMeshAgent navAgent;
    private Transform transform;
    private Enemy enemy;

    private float speed;
    private float acceleration;

    private float warpDistMin = 10;
    private float warpDistMax = 15;

    private float blindSpotAngle = 45;

    private bool hasWarped = false;

    private AudioClip warpSound;

    public TaskStalkWarpBehind(aa_Stalk owner, Enemy enemy, float speed = 20, float acceleration = 400)
    {
        this.owner = owner;
        this.enemy = enemy;
        navAgent = enemy.navAgent;
        transform = navAgent.transform;
        this.speed = speed;
        this.acceleration = acceleration;
    }
    public override Status Check(float dt)
    {
        if (owner.stalkAttemptCounter > 0)
        {
            if (!hasWarped)
            {
                float pAng = owner.currentTargetPlayer.transform.rotation.eulerAngles.y;
                float randAng = Random.Range(pAng + blindSpotAngle, pAng + (360 - blindSpotAngle)) % 360;
                float randDist = Random.Range(warpDistMin, warpDistMax);
                Vector3 loc = new Vector3(Mathf.Sin(randAng) * randDist, 0, Mathf.Cos(randAng) * randDist) + owner.currentTargetPlayer.transform.position;

                NavMeshHit hit;
                if (NavMesh.SamplePosition(loc, out hit, 10, NavMesh.AllAreas))
                    navAgent.Warp(hit.position);

                navAgent.speed = 0;

                hasWarped = true;

                // TEMPORARY
                enemy.PlaySound("musicLover");
            }

            status = Status.SUCCESS;
            return status;
        }

        status = Status.FAILURE;
        return status;
    }

    protected override void OnResetNode()
    {
        hasWarped = false;
        base.OnResetNode();
    }
}
