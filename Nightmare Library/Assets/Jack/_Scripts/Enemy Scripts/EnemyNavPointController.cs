using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    public static EnemyNavPointController GetClosestNavPoint(Vector3 pos)
    {
        // TEMPORARY!!! using this method to save memory, I do know that this isn't the distance formula
        float minDistance = float.MaxValue;
        EnemyNavPointController closest = null;

        foreach (EnemyNavPointController e in enemyNavPoints)
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
    public static EnemyNavPointController GetFarthestNavPoint(Vector3 pos)
    {
        // TEMPORARY!!! using this method to save memory, I do know that this isn't the distance formula
        float maxDistance = float.MinValue;
        EnemyNavPointController farthest = null;

        foreach (EnemyNavPointController e in enemyNavPoints)
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
    public static EnemyNavPointController GetRandomNavPoint()
    {
        return enemyNavPoints[Random.Range(0, enemyNavPoints.Count)];
    }
    public static EnemyNavPointController GetRandomNavPoint(EnemyNavPointController exclude)
    {
        List<EnemyNavPointController> newList = new List<EnemyNavPointController>(enemyNavPoints);
        newList.Remove(exclude);
        return newList[Random.Range(0, newList.Count)];
    }

    private void OnDestroy()
    {
        enemyNavPoints.Remove(this);
    }
}
