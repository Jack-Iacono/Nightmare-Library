using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;
using UnityEngine.AI;
using static UnityEngine.UI.GridLayoutGroup;

public class AlertNodeVariables
{
    // This contains all the variables that need to be present in order for the alert system to work
    public List<Vector3> queue;
    public int maxQueueSize;

    public AlertNodeVariables(int maxQueueSize)
    {
        this.maxQueueSize = maxQueueSize;
        queue = new List<Vector3>();
    }

    public void AddQueueItem(Vector3 item)
    {
        if (!queue.Contains(item))
        {
            if (queue.Count >= maxQueueSize)
                queue.RemoveAt(queue.Count - 1);
            queue.Insert(0, item);
        }
    }
    public void SetQueueSize(int size)
    {
        maxQueueSize = size;
    }
}
public class Node_AlertGoto : Node
{
    AlertNodeVariables data;
    Enemy enemy;
    NavMeshAgent agent;
    Transform transform;

    public Node_AlertGoto(AlertNodeVariables data, Enemy enemy)
    {
        this.data = data;
        this.enemy = enemy;
        transform = enemy.transform;
        agent = enemy.GetComponent<NavMeshAgent>(); 
    }
    public override Status Check(float dt)
    {
        agent.destination = data.queue[0];

        if (Vector3.Distance(agent.destination, transform.position) < 2)
        {
            status = Status.SUCCESS;
            return status;
        }

        status = Status.RUNNING;
        return status;
    }
}
public class Node_AlertRemoveFirst : Node
{
    AlertNodeVariables data;
    public Node_AlertRemoveFirst(AlertNodeVariables data)
    {
        this.data = data;
    }
    public override Status Check(float dt)
    {
        data.queue.RemoveAt(0);
        return Status.SUCCESS;
    }
}
public class Node_AlertClear : Node
{
    AlertNodeVariables data;
    public Node_AlertClear(AlertNodeVariables data)
    {
        this.data = data;
    }
    public override Status Check(float dt)
    {
        data.queue = new List<Vector3>();
        return Status.SUCCESS;
    }
}
public class Node_AlertCheck : Node
{
    AlertNodeVariables data;
    public Node_AlertCheck(AlertNodeVariables data)
    {
        this.data = data;
    }
    public override Status Check(float dt)
    {
        return data.queue.Count > 0 ? Status.SUCCESS : Status.FAILURE;
    }
}
