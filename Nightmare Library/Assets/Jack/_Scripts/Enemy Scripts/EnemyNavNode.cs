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

    // May need to re-add 1 << 12 | 
    private LayerMask pointLayers = 1 << 9;

    // TESTING ONLY
    public int nodeStatus { get; set; } = -1;

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

        RaycastHit hit;

        // If nothing is blocking this path, add the node to the neighbors list
        if (!Physics.Raycast(ray, out hit, dist, pointLayers))
        {
            neighbors.Add(p, dist);

            // Visualizes the paths
            //Debug.DrawRay(ray.origin, ray.direction * dist, Color.green, 10);
        }
    }
    public void RayToNode(EnemyNavNode node)
    {
        Ray ray = new Ray(position, node.position - position);
        Debug.DrawRay(ray.origin, ray.direction * Vector3.Distance(node.position, position), Color.black, 2f);
    }

    public EnemyNavNode GetRandomNeighbor(List<EnemyNavNode> exclude)
    {
        List<EnemyNavNode> valid = new List<EnemyNavNode>();

        foreach(EnemyNavNode node in neighbors.Keys)
        {
            if (!exclude.Contains(node))
                valid.Add(node);
        }

        if (valid.Count > 0)
            return valid[Random.Range(0, valid.Count)];
        else
            return null;
    }

    private void OnDestroy()
    {
        EnemyNavGraph.Remove(this);
    }

    private void OnDrawGizmos()
    {
        switch (nodeStatus)
        {
            case 0:
                Gizmos.color = Color.grey;
                break;
            case 1:
                Gizmos.color = Color.red;
                break;
            case 2:
                Gizmos.color = Color.green;
                break;
        }

        Gizmos.DrawSphere(transform.position, 2f);
    }
}
