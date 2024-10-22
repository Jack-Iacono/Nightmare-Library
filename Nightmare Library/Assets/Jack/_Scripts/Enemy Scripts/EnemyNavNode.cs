using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class EnemyNavNode : MonoBehaviour
{
    public Vector3 position { get; private set; } = Vector3.zero;
    public Dictionary<EnemyNavNode, float> neighbors = new Dictionary<EnemyNavNode, float>();

    private LayerMask pointLayers = 1 << 12 | 1 << 9;

    private void Awake()
    {
        position = transform.position;
        EnemyNavGraph.Add(this);
    }

    public void CheckNeighbor(EnemyNavNode p)
    {
        Ray ray = new Ray(position, (p.position - position).normalized);
        float dist = Vector3.Distance(p.position, position);

        // If nothing is blocking this path
        if (!Physics.Raycast(ray, dist, pointLayers))
        {
            float modDist = 10000 / (dist * dist + 1000);
            neighbors.Add(p, modDist);

            // Visualizes the paths
            // Debug.DrawRay(ray.origin, ray.direction * dist, Color.black, 10);
        }
    }
    public void RayToNode(EnemyNavNode node)
    {
        Ray ray = new Ray(position, node.position - position);
        Debug.DrawRay(ray.origin, ray.direction * Vector3.Distance(node.position, position), Color.black, 2f);
    }

    private void OnDestroy()
    {
        EnemyNavGraph.Remove(this);
    }
}
