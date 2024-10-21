using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class EnemyNavPoint : MonoBehaviour
{
    public Vector3 position { get; private set; } = Vector3.zero;
    public Dictionary<EnemyNavPoint, float> neighbors = new Dictionary<EnemyNavPoint, float>();
    public List<EnemyNavPoint> n = new List<EnemyNavPoint>();

    private LayerMask pointLayers = 1 << 12 | 1 << 9;

    private void Awake()
    {
        position = transform.position;
        EnemyNavGraph.Add(this);
    }

    public void CheckNeighbor(EnemyNavPoint p)
    {
        Ray ray = new Ray(position, (p.position - position).normalized);
        float dist = Vector3.Distance(p.position, position);

        // If nothing is blocking this path
        if (!Physics.Raycast(ray, dist, pointLayers))
        {
            float modDist = 10000 / (dist * dist + 1000);
            neighbors.Add(p, modDist);
            n.Add(p);

            // Visualizes the paths
            Debug.DrawRay(ray.origin, ray.direction * dist, Color.black, 10);
        }
    }

    private void OnDestroy()
    {
        EnemyNavGraph.Remove(this);
    }
}
