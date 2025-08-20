using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.CompilerServices;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UIElements;

public static class EnemyNavGraph
{
    public static List<EnemyNavNode> enemyNavPoints = new List<EnemyNavNode>();
    
    public static void Add(EnemyNavNode point)
    {
        // Add the new point to the list
        enemyNavPoints.Add(point);

        // Run through all points in the list except for the new one
        for(int i = 0; i  < enemyNavPoints.Count - 1; i++)
        {
            // Check all previous points against the new point
            enemyNavPoints[i].CheckNeighbor(point);

            // Check the new point against all previous points
            bool added = point.CheckNeighbor(enemyNavPoints[i]);
        }
    }
    public static void Remove(EnemyNavNode point)
    {
        enemyNavPoints.Remove(point);
    }

    public static EnemyNavNode GetClosestNavPoint(Vector3 pos)
    {
        float minDistance = float.MaxValue;
        EnemyNavNode closest = null;

        foreach (EnemyNavNode e in enemyNavPoints)
        {
            float dist = Vector3.Distance(e.position, pos);
            if (dist < minDistance)
            {
                closest = e;
                minDistance = dist;
            }
        }
        return closest;
    }

    public static EnemyNavNode GetClosestNavPointRay(Vector3 pos, EnemyNavNode[] exclude)
    {
        float minDistance = float.MaxValue;
        EnemyNavNode closest = null;

        foreach (EnemyNavNode e in enemyNavPoints)
        {
            if (!exclude.Contains(e))
            {
                float dist = Vector3.Distance(e.position, pos);
                Ray ray = new Ray(pos, (e.position - pos));

                // May need to change this later to accomodate for layer shifting
                if (dist < minDistance && !Physics.Raycast(ray.origin, ray.direction, dist, 1 << 9))
                {
                    closest = e;
                    minDistance = dist;
                }
            }
        }
        return closest;
    }
    public static EnemyNavNode GetClosestNavPointRay(Vector3 pos)
    {
        EnemyNavNode node = GetClosestNavPointRay(pos, new EnemyNavNode[0]);
        if (node == null)
            node = GetClosestNavPoint(pos);
        return node;
    }

    public static EnemyNavNode GetFarthestNavPoint(Vector3 pos)
    {
        // TEMPORARY!!! using this method to save memory, I do know that this isn't the distance formula
        float maxDistance = float.MinValue;
        EnemyNavNode farthest = null;

        foreach (EnemyNavNode e in enemyNavPoints)
        {
            float dist = Mathf.Abs(pos.x - e.position.x) + Mathf.Abs(pos.y - e.position.y) + Mathf.Abs(pos.z - e.position.z);
            if (dist > maxDistance)
            {
                farthest = e;
                maxDistance = dist;
            }
        }
        return farthest;
    }
    public static EnemyNavNode GetRandomNavPoint()
    {
        return enemyNavPoints[Random.Range(0, enemyNavPoints.Count)];
    }
    public static EnemyNavNode GetRandomNavPoint(EnemyNavNode exclude)
    {
        List<EnemyNavNode> newList = new List<EnemyNavNode>(enemyNavPoints);
        newList.Remove(exclude);
        return newList[Random.Range(0, newList.Count)];
    }

    public static EnemyNavNode GetOutOfSightNode(PlayerController player)
    {
        EnemyNavNode closest = GetClosestNavPoint(player.transform.position);
        Transform playerTrans = player.transform;

        List<EnemyNavNode> viewedNodes = new List<EnemyNavNode>();

        // Create a queue to hold all the neighbors being viewed
        PriorityQueue<EnemyNavNode> priorityQueue = new PriorityQueue<EnemyNavNode>();
        priorityQueue.Insert(new PriorityQueue<EnemyNavNode>.Element(closest, 0));

        // A list of all the nodes that are valid in this context
        List<EnemyNavNode> validNodes = new List<EnemyNavNode>();

        while (priorityQueue.Count > 0 && validNodes.Count < 5)
        {
            // Distance represents amount of neighbors away from the closest node
            int dist = priorityQueue.Front();
            EnemyNavNode current = priorityQueue.Extract();

            RaycastHit hit;
            Ray playerToNode = new Ray(playerTrans.position, (current.position - playerTrans.position).normalized);

            // Check if this is the closest node, if so, get rid of it
            // Could I do all of this in one if statement, yes, do I want to do that, no
            if (current != closest)
            {
                // Check if the node is out of view
                if (Vector3.Dot(playerTrans.forward, playerToNode.direction) <= 0.65f)
                {
                    validNodes.Add(current);
                }
                // Check to see if there is something in between the player and the selected node
                else if (Physics.Raycast(playerToNode.origin, playerToNode.direction, out hit, Vector3.Distance(playerTrans.position, current.position), 1 << 6 | 1 << 9))
                {
                    Debug.Log(hit.collider.name);
                    // Check to make sure that something is in between this object and the player
                    if (hit.collider.gameObject != player.gameObject)
                    {
                        validNodes.Add(current);
                    }
                }
            }
            
            // This node has been viewed already
            viewedNodes.Add(current);

            // Send in all nodes that haven't been visited yet
            foreach (EnemyNavNode node in current.neighbors.Keys)
            {
                if (!viewedNodes.Contains(node) && !priorityQueue.Contains(node))
                {
                    priorityQueue.Insert(new PriorityQueue<EnemyNavNode>.Element(node, dist + 1));
                    viewedNodes.Add(node);
                }
            }
        }

        // If there are valid nodes, pick a random one
        if (validNodes.Count > 0)
            return validNodes[Random.Range(0, validNodes.Count)];

        // If no nodes are valid, just return a random neighbor of he closest node
        return closest.GetRandomNeighbor(null);
    }
    public static EnemyNavNode[] GetClosestNodePair(Vector3 pos)
    {
        try
        {
            EnemyNavNode[] closest = new EnemyNavNode[2];

            // Get the closest node to the player
            closest[0] = GetClosestNavPointRay(pos);

            Vector3 closeDirection = (pos - closest[0].position).normalized;

            float lowAngle = -1;
            EnemyNavNode closestNode = null;

            // Run through that node's neighbors and check which is closest
            foreach (EnemyNavNode checkNode in closest[0].neighbors.Keys)
            {
                // Get the dot product of the ray pointing toward the player and the ray from the closest node to the current candidate
                float dotProd = Vector3.Dot(closeDirection, (checkNode.position - closest[0].position).normalized);

                // If this angle is more shallow than the prior, make it the new goal
                if (dotProd >= lowAngle)
                {
                    lowAngle = dotProd;
                    closestNode = checkNode;
                }
            }

            closest[1] = closestNode;

            return closest;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the path from one node to another using neighboring nodes
    /// </summary>
    /// <param name="start">The node that the user is currently at</param>
    /// <param name="goal">The node that the user wants to go to</param>
    /// <returns>A list of EnemyNavPoints that will get the user from the starting point to the goal point</returns>
    public static List<EnemyNavNode> GetPathToPoint(EnemyNavNode start, EnemyNavNode goal)
    {
        // Ooh look at me, I used an A* search algo for no real reason
        // It's because I wanted to bitch

        // Create a Priority Queue to hold the nodes
        PriorityQueue<EnemyNavNode> nodes = new PriorityQueue<EnemyNavNode>();
        nodes.Insert(new PriorityQueue<EnemyNavNode>.Element(start, 0));

        // Create a Dictionary for backtracking later and for keeping track of costs
        Dictionary<EnemyNavNode, EnemyNavNode> cameFrom = new Dictionary<EnemyNavNode, EnemyNavNode>();
        Dictionary<EnemyNavNode, int> costSoFar = new Dictionary<EnemyNavNode, int>();    

        // Initialize both lists with the starting location
        cameFrom.Add(start, null);
        costSoFar.Add(start, 0);

        // Continue until you are out of nodes
        while(nodes.Count > 0)
        {
            // Get the next closest node
            EnemyNavNode current = nodes.Extract();

            // If we are at the goal, stop running
            if (current == goal) break;

            // Run through all of this node's neighbors
            foreach(EnemyNavNode next in current.neighbors.Keys)
            {
                // Get the cost to move to the next node and add to the cost to get to this node
                float newCost = costSoFar[current] + 1;
                //float newCost = costSoFar[current] + current.neighbors[next];

                // Check if there is a cost associated with this node, if yes, check to see if the new cost to get there is less than the one already present
                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = (int)newCost;
                    // Would add Heuristic here to convert to A*, may add later
                    int priority = (int)newCost;
                    nodes.Insert(new PriorityQueue<EnemyNavNode>.Element(next, priority));
                    cameFrom[next] = current;
                }
            }
        }

        // Reconstruct the path
        List<EnemyNavNode> path = new List<EnemyNavNode>();
        EnemyNavNode evalPoint = goal;

        while (true)
        {
            path.Insert(0, evalPoint);
            if (cameFrom.ContainsKey(evalPoint) && cameFrom[evalPoint] != null)
                evalPoint = cameFrom[evalPoint];
            else
                break;
        }

        return path;
    }
}
