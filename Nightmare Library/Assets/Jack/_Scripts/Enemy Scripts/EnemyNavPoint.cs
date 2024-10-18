using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class EnemyNavPoint : MonoBehaviour
{
    public Vector3 position { get; private set; } = Vector3.zero;
    public List<EnemyNavPoint> neighbors;

    private LayerMask pointLayers = 1 << 12 | 1 << 9;

    private void Awake()
    {
        position = transform.position;
        EnemyNavGraph.Add(this);
    }

    public void CheckNeighbor(EnemyNavPoint p)
    {
        RaycastHit hit;
        Ray ray = new Ray(position, (p.position - position).normalized);

        if(Physics.Raycast(ray, out hit, 1000, pointLayers))
        {
            if(hit.collider.gameObject == p.gameObject)
            {
                neighbors.Add(p);

                // Visualizes the paths
                //float dist = Vector3.Distance(position, p.position);
                //Debug.DrawRay(ray.origin, ray.direction * dist, Color.yellow, 10);
            }
        }
    }

    private void OnDestroy()
    {
        EnemyNavGraph.Remove(this);
    }
}
