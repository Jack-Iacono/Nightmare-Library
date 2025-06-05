using BehaviorTree;
using UnityEngine;
using UnityEngine.AI;

public class Node_GotoTargetDynamic : Node
{
    private ActiveAttack owner;
    private NavMeshAgent navAgent;
    private Transform transform;

    public Node_GotoTargetDynamic(ActiveAttack owner, Enemy enemy)
    {
        this.owner = owner;
        transform = enemy.transform;
        navAgent = enemy.navAgent;
    }

    public override Status Check(float dt)
    {
        // Check if the agent is still not at the target
        if (Vector3.Distance(transform.position, owner.currentTargetDynamic.position) > 0.5f)
        {
            navAgent.speed = (float)GetData("speed") * 2;
            navAgent.destination = owner.currentTargetDynamic.position;
            status = Status.RUNNING;
            return status;
        }

        navAgent.speed = (float)GetData("speed");
        status = Status.SUCCESS;
        return status;
    }
}
