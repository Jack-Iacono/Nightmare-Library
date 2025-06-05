using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;
using System.IO;

public class PathNodeData
{
    private Vector3 currentNode;
    private List<Vector3> nodeQueue = new List<Vector3>();
    private List<Vector3> currentPath = new List<Vector3>();

    public void AddNode(EnemyNavNode node)
    {
        nodeQueue.Add(node.position);
    }
    public void NextNode()
    {
        nodeQueue.RemoveAt(0);
    }
    public Vector3 GetCurrentNode()
    {
        return currentNode;
    }
    public Vector3 GetQueueNode(int i)
    {
        return nodeQueue[i];
    }

    public void SetPath(List<Vector3> navNodes)
    {
        this.currentPath = navNodes;
    }
    public void RemovePathNode()
    {
        currentPath.RemoveAt(0);
    }
    public Vector3 GetPathNode()
    {
        return currentPath[0];
    }
    public bool PathEmpty()
    {
        return currentPath.Count <= 0;
    }
}

public class Node_RefreshPath : Node
{
    private PathNodeData data;
    public Node_RefreshPath(PathNodeData data)
    {
        this.data = data;
    }
    public override Status Check(float dt)
    {
        // Get the path that the enemy will now follow
        //data.SetPath(EnemyNavGraph.GetPathToPoint(data.GetCurrentNode(), data.GetQueueNode(0)));

        return Status.SUCCESS;
    }
}
