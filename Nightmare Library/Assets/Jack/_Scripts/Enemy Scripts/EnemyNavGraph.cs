using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnemyNavGraph
{
    public static List<EnemyNavPoint> enemyNavPoints = new List<EnemyNavPoint>();
    
    public static void Add(EnemyNavPoint point)
    {
        // Add the new point to the list
        enemyNavPoints.Add(point);

        // Run through all points in the list except for the new one
        for(int i = 0; i  < enemyNavPoints.Count - 1; i++)
        {
            // Check all previous points against the new point
            enemyNavPoints[i].CheckNeighbor(point);
            // Check the new point against all previous points
            point.CheckNeighbor(enemyNavPoints[i]);
        }
    }
    public static void Remove(EnemyNavPoint point)
    {
        enemyNavPoints.Remove(point);
    }

    public static EnemyNavPoint GetClosestNavPoint(Vector3 pos)
    {
        // TEMPORARY!!! using this method to save memory, I do know that this isn't the distance formula
        float minDistance = float.MaxValue;
        EnemyNavPoint closest = null;

        foreach (EnemyNavPoint e in enemyNavPoints)
        {
            float dist = Mathf.Abs(pos.x - e.position.x) + Mathf.Abs(pos.y - e.position.y) + Mathf.Abs(pos.z - e.position.z);
            if (dist < minDistance)
            {
                closest = e;
                minDistance = dist;
            }
        }
        return closest;
    }
    public static EnemyNavPoint GetFarthestNavPoint(Vector3 pos)
    {
        // TEMPORARY!!! using this method to save memory, I do know that this isn't the distance formula
        float maxDistance = float.MinValue;
        EnemyNavPoint farthest = null;

        foreach (EnemyNavPoint e in enemyNavPoints)
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
    public static EnemyNavPoint GetRandomNavPoint()
    {
        return enemyNavPoints[Random.Range(0, enemyNavPoints.Count)];
    }
    public static EnemyNavPoint GetRandomNavPoint(EnemyNavPoint exclude)
    {
        List<EnemyNavPoint> newList = new List<EnemyNavPoint>(enemyNavPoints);
        newList.Remove(exclude);
        return newList[Random.Range(0, newList.Count)];
    }

    /// <summary>
    /// Gets the path from one node to another using neighboring nodes
    /// </summary>
    /// <param name="start">The node that the user is currently at</param>
    /// <param name="goal">The node that the user wants to go to</param>
    /// <returns>A list of EnemyNavPoints that will get the user from the starting point to the goal point</returns>
    public static List<EnemyNavPoint> GetPathToPoint(EnemyNavPoint start, EnemyNavPoint goal)
    {
        // Create a Priority Queue to hold the nodes
        PriorityQueue<EnemyNavPoint> nodes = new PriorityQueue<EnemyNavPoint>();
        nodes.Insert(new PriorityQueue<EnemyNavPoint>.Element(start, 0));

        // Create a Dictionary for backtracking later and for keeping track of costs
        Dictionary<EnemyNavPoint, EnemyNavPoint> cameFrom = new Dictionary<EnemyNavPoint, EnemyNavPoint>();
        Dictionary<EnemyNavPoint, int> costSoFar = new Dictionary<EnemyNavPoint, int>();    

        // Initialize both lists with the starting location
        cameFrom.Add(start, null);
        costSoFar.Add(start, 0);

        // Continue until you are out of nodes
        while(nodes.Count > 0)
        {
            // Get the next closest node
            EnemyNavPoint current = nodes.Extract();

            // If we are at the goal, stop running
            if (current == goal) break;

            // Run through all of this node's neighbors
            foreach(EnemyNavPoint next in current.neighbors.Keys)
            {
                // Get the cost to move to the next node and add to the cost to get to this node
                float newCost = costSoFar[current] + current.neighbors[next];

                // Check if there is a cost associated with this node, if yes, check to see if the new cost to get there is less than the one already present
                if(!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = (int)newCost;
                    // Would add Heuristic here to convert to A*, may add later
                    int priority = (int)newCost;
                    nodes.Insert(new PriorityQueue<EnemyNavPoint>.Element(next, priority));
                    cameFrom[next] = current;
                }
            }
        }

        // Reconstruct the path
        List<EnemyNavPoint> path = new List<EnemyNavPoint>();
        EnemyNavPoint evalPoint = goal;

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
