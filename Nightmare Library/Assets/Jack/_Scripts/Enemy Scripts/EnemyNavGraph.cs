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
}
