using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TaskWardenCheckAlert : Node
{
    Vector3 currentTarget = Vector3.zero;
    aa_Warden owner;
    NavMeshAgent agent;
    Transform transform;

    public TaskWardenCheckAlert(aa_Warden owner, Enemy enemy)
    {
        this.owner = owner;
        agent = enemy.navAgent;
        transform = enemy.transform;
    }
    public override Status Check(float dt)
    {
        currentTarget = owner.GetAlertItem();

        agent.destination = currentTarget;

        if (Vector3.Distance(currentTarget, transform.position) < 2)
        {
            status = Status.SUCCESS;
            return status;
        }

        status = Status.RUNNING;
        return status;
    }
}
