using BehaviorTree;
using UnityEngine;
using UnityEngine.AI;

public class TaskChasePlayer : Node
{
    private Transform player;
    private NavMeshAgent navAgent;
    private Transform transform;

    public TaskChasePlayer(Transform transform, NavMeshAgent navAgent)
    {
        player = PlayerController.playerInstances[0].transform;
        this.transform = transform;
        this.navAgent = navAgent;
    }

    public override Status Check(float dt)
    {
        // Get the current target node
        Vector3 target = (Vector3)GetData("playerKnownPosition");

        // Check if the agent is still not at the target
        if (Vector3.Distance(transform.position, target) > 0.5f)
        {
            navAgent.speed = (float)GetData("speed") * 2;
            navAgent.destination = target;
            status = Status.RUNNING;
            return status;
        }

        navAgent.speed = (float)GetData("speed");
        status = Status.SUCCESS;
        return status;
    }
}
