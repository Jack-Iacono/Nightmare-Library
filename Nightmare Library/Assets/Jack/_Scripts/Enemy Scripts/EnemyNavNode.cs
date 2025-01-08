using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyNavNode : MonoBehaviour
{
    public Vector3 position { get; private set; } = Vector3.zero;
    public Dictionary<EnemyNavNode, float> neighbors { get; private set; } = new Dictionary<EnemyNavNode, float>();

    [Tooltip("Force this node to connect to another node even if they do not have line of sight")]
    [SerializeField]
    private List<EnemyNavNode> neighborOverrides = new List<EnemyNavNode>();

    private LayerMask pointLayers = 1 << 12 | 1 << 9;

    private void Awake()
    {
        position = transform.position;
        EnemyNavGraph.Add(this);

        foreach(EnemyNavNode node in neighborOverrides)
        {
            neighbors.Add(node, Vector3.Distance(position, node.position));
        }
    }

    public void CheckNeighbor(EnemyNavNode p)
    {
        // Make sure not to add if the node is already present
        if (neighbors.ContainsKey(p))
            return;

        Ray ray = new Ray(position, (p.position - position).normalized);
        float dist = Vector3.Distance(p.position, position);

        // If nothing is blocking this path, add the node to the neighbors list
        if (!Physics.Raycast(ray, dist, pointLayers))
        {
            neighbors.Add(p, dist);

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
