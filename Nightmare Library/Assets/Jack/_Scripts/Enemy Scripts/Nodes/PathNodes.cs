using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;
using System.IO;
using UnityEngine.AI;

public class PathNodeData
{
    private EnemyNavNode currentNode;
    private List<EnemyNavNode> nodeQueue = new List<EnemyNavNode>();
    private List<EnemyNavNode> currentPath = new List<EnemyNavNode>();

    private List<EnemyNavNode> visitedNodes = new List<EnemyNavNode>();

    private NavMeshAgent agent;
    private Transform trans;

    public PathNodeData(Enemy enemy)
    {
        // Sets this path system up to be used
        RestartQueue();
        this.agent = enemy.navAgent;
        this.trans = enemy.transform;
        agent.Warp(currentNode.position);
    }

    public NavMeshAgent GetAgent() { return agent; }
    public Transform GetTransform() { return trans; }

    public void AddQueueNode(EnemyNavNode node)
    {
        nodeQueue.Add(node);
    }
    public EnemyNavNode GetNextNode()
    {
        return nodeQueue[0];
    }
    public EnemyNavNode GetCurrentNode()
    {
        return currentNode;
    }
    public bool QueueEmpty()
    {
        return nodeQueue.Count <= 0;
    }

    public EnemyNavNode ExtractPathNode()
    {
        EnemyNavNode node = nodeQueue[0];
        nodeQueue.RemoveAt(0);
        return node;
    }
    public void SetPath(List<EnemyNavNode> navNodes)
    {
        this.currentPath = navNodes;
    }
    public void RemovePathNode()
    {
        currentPath.RemoveAt(0);
    }
    public bool PathEmpty()
    {
        return currentPath.Count <= 0;
    }

    public void VisitCurrentNode()
    {
        visitedNodes.Add(currentNode);
        currentNode = nodeQueue[0];
        nodeQueue.RemoveAt(0);
    }
    public void ClearVisitedNodes()
    {
        visitedNodes.Clear();
    }
    public List<EnemyNavNode> GetVisitedNodes()
    {
        return visitedNodes;
    }

    public void RestartQueue()
    {
        if (currentNode == null)
            currentNode = EnemyNavGraph.GetRandomNavPoint();

        visitedNodes.Clear();
        nodeQueue.Clear();

        nodeQueue.Add(EnemyNavGraph.GetFarthestNavPoint(currentNode.position));
        nodeQueue.Add(currentNode.GetRandomNeighbor(null));
    }
}

public class Node_PathSet : Node
{
    private PathNodeData data;
    private bool hasSetPath = false;
    public Node_PathSet(PathNodeData data)
    {
        this.data = data;
    }
    public override Status Check(float dt)
    {
        // Get the path that the enemy will now follow
        if (!hasSetPath)
        {
            data.SetPath(EnemyNavGraph.GetPathToPoint(data.GetCurrentNode(), data.GetNextNode()));
            hasSetPath = true;
        }
        
        return Status.SUCCESS;
    }
    protected override void OnResetNode()
    {
        hasSetPath = false;
        base.OnResetNode();
    }
}
public class Node_PathTraverse : Node
{
    private PathNodeData data;

    private bool waiting = false;
    private float atNodeWaitTime = 0.5f;
    private float atNodeWaitTimer = 0.5f;

    private NavMeshAgent agent;
    private Transform trans;

    private float speed;

    private EnemyNavNode currentTargetNode = null;

    public Node_PathTraverse(PathNodeData data, float timer, float speed)
    {
        this.data = data;

        atNodeWaitTime = timer;
        atNodeWaitTimer = timer;

        agent = data.GetAgent();
        trans = data.GetTransform();

        this.speed = speed;
    }
    public override Status Check(float dt)
    {
        if (currentTargetNode == null)
            currentTargetNode = data.ExtractPathNode();

        // Wait after getting to a node
        if (waiting)
        {
            if (atNodeWaitTimer > 0)
                atNodeWaitTimer -= dt;
            else
            {
                atNodeWaitTimer = atNodeWaitTime;
                waiting = false;
            }
        }
        else if (currentTargetNode != null)
        {
            agent.destination = currentTargetNode.position;
            agent.speed = speed;
            agent.acceleration = speed * 5;

            // Is the player close enough to the target node
            if (Vector3.SqrMagnitude(trans.position - currentTargetNode.position) < 9)
            {
                waiting = true;
                currentTargetNode = data.ExtractPathNode();
            }
        }
        else
        {
            status = Status.SUCCESS;
            return status;
        }

        status = Status.RUNNING;
        return status;
    }
}
public class Node_PathComplete : Node
{
    private PathNodeData data;
    private bool refillQueue = true;

    public Node_PathComplete(PathNodeData data, bool refillQueue)
    {
        this.data = data;
        this.refillQueue = refillQueue;
    }
    public override Status Check(float dt)
    {
        data.VisitCurrentNode();

        if (!data.QueueEmpty())
        {
            if (refillQueue)
            {
                EnemyNavNode tempNode = data.GetCurrentNode().GetRandomNeighbor(data.GetVisitedNodes());

                // If valid node was found, continue with that, if not, clear the visited nodes and pick again
                if (tempNode != null)
                {
                    data.AddQueueNode(tempNode);
                }
                else
                {
                    data.RestartQueue();
                }
            }
        }
        else
        {
            // Run as a fresh start with a random far patrol point
            data.RestartQueue();
        }

        status = Status.SUCCESS;
        return status;
    }
}
