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

    private float speed;
    private float acceleration;

    private float blindSpotAngle = 30;

    private bool hasWarped = false;

    public TaskStalkWarpBehind(aa_Stalk owner, NavMeshAgent navAgent, float speed = 20, float acceleration = 400)
    {
        this.owner = owner;
        this.navAgent = navAgent;
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
                float randDist = Random.Range(owner.closeInRange - 2, owner.closeInRange);
                Vector3 loc = new Vector3(Mathf.Sin(randAng) * randDist, 0, Mathf.Cos(randAng) * randDist) + owner.currentTargetPlayer.transform.position;

                NavMeshHit hit;
                if (NavMesh.SamplePosition(loc, out hit, 10, NavMesh.AllAreas))
                    navAgent.Warp(hit.position);

                navAgent.speed = 0;

                hasWarped = true;
                Debug.Log("Make Noise");
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
