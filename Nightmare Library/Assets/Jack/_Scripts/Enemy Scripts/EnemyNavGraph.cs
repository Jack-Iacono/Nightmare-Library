using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class EnemyNavGraph
{
    public static List<EnemyNavNode> enemyNavPoints = new List<EnemyNavNode>();
    private static List<NeighborPair> neighborPairs = new List<NeighborPair>();
    
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

            if(added)
                AddNeighborPair(enemyNavPoints[i], point);
        }
    }
    public static void Remove(EnemyNavNode point)
    {
        RemoveNeighborPairs(point);
        enemyNavPoints.Remove(point);
    }

    private static void AddNeighborPair(EnemyNavNode node1 ,EnemyNavNode node2)
    {
        // Adds this pair if it not already present
        NeighborPair pair = new NeighborPair(node1, node2);
        if (!neighborPairs.Contains(pair))
        {
            neighborPairs.Add(pair);
        }
    }
    private static void RemoveNeighborPairs(EnemyNavNode node)
    {
        for(int i = neighborPairs.Count - 1; i > 0; i--)
        {
            if (neighborPairs[i].Contains(node))
                neighborPairs.RemoveAt(i);
        }
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

    public static NeighborPair GetClosestNodePair(Vector3 pos)
    {
        NeighborPair closestPair = null;
        float minDistance = 10000;

        foreach(NeighborPair pair in neighborPairs)
        {
            Ray ray = pair.GetRay();
            Vector3 closestPointOnRay = ray.origin + Vector3.Dot(pos - ray.origin, ray.direction) * ray.direction;
            float distanceToRay = Vector3.Distance(pos, closestPointOnRay);

            if(distanceToRay < minDistance)
            {
                minDistance = distanceToRay;
                closestPair = pair;
            }
        }

        return closestPair;
    }

    /// <summary>
    /// Gets the path from one node to another using neighboring nodes
    /// </summary>
    /// <param name="start">The node that the user is currently at</param>
    /// <param name="goal">The node that the user wants to go to</param>
    /// <returns>A list of EnemyNavPoints that will get the user from the starting point to the goal point</returns>
    public static List<EnemyNavNode> GetPathToPoint(EnemyNavNode start, EnemyNavNode goal)
    {
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

    public class NeighborPair
    {
        public EnemyNavNode node1;
        public EnemyNavNode node2;

        private Ray ray;
        private float length;

        public bool Contains(EnemyNavNode node)
        {
            return node1 == node || node2 == node;
        }

        public NeighborPair(EnemyNavNode node1, EnemyNavNode node2)
        {
            this.node1 = node1;
            this.node2 = node2;

            ray = new Ray(node1.position, node2.position - node1.position);
            length = Vector3.Distance(node1.position, node2.position);

            Debug.DrawRay(ray.origin, ray.direction * length, Color.cyan, 10);
        }

        public Ray GetRay() { return ray; }

        public override bool Equals(object obj)
        {
            NeighborPair other = obj as NeighborPair;

            if((other.node1 == node1 && other.node2 == node2) || (other.node2 == node1 && other.node1 == node2))
            {
                return true;
            }

            return false;
        }
        public override int GetHashCode()
        {
            return node1.GetHashCode() + node2.GetHashCode();
        }

        public override string ToString()
        {
            return "Node 1: " + node1.ToString() + " || Node 2: " + node2.ToString();
        }
    }
}
