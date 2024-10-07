using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class EnemyNavPointController : MonoBehaviour
{
    public static List<EnemyNavPointController> enemyNavPoints = new List<EnemyNavPointController>();
    public Vector3 position { get; private set; } = Vector3.zero;

    private void Awake()
    {
        enemyNavPoints.Add(this);
        position = transform.position;
    }

    public static Vector3 GetClosestNavPoint(Vector3 pos)
    {
        // TEMPORARY!!! using this method to save memory, I do know that this isn't the distance formula
        float minDistance = float.MaxValue;
        Vector3 closest = Vector3.zero;

        foreach(EnemyNavPointController e in enemyNavPoints)
        {
            float dist = Mathf.Abs(pos.x - e.position.x) + Mathf.Abs(pos.y - e.position.y) + Mathf.Abs(pos.z - e.position.z);
            if(dist < minDistance)
            {
                closest = e.position;
                minDistance = dist;
            }
        }
        return closest;
    }
    public static Vector3 GetFarthestNavPoint(Vector3 pos)
    {
        // TEMPORARY!!! using this method to save memory, I do know that this isn't the distance formula
        float maxDistance = float.MinValue;
        Vector3 farthest = Vector3.zero;

        foreach (EnemyNavPointController e in enemyNavPoints)
        {
            float dist = Mathf.Abs(pos.x - e.position.x) + Mathf.Abs(pos.y - e.position.y) + Mathf.Abs(pos.z - e.position.z);
            if (dist > maxDistance)
            {
                farthest = e.position;
                maxDistance = dist;
            }
        }
        return farthest;
    }
    public static Vector3 GetRandomNavPoint()
    {
        return enemyNavPoints[Random.Range(0, enemyNavPoints.Count)].position;
    }

    private void OnDestroy()
    {
        enemyNavPoints.Remove(this);
    }
}
