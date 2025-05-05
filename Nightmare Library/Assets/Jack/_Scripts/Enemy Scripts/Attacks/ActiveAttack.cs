using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class ActiveAttack : Attack
{
    protected BehaviorTree.Tree tree;

    // These variables are used among almost all attacks
    public Transform currentTargetDynamic { get; protected set; } = null;
    public Vector3 currentTargetStatic { get; protected set; } = Vector3.zero;
    public static readonly LayerMask envLayers = 1 << 9 | 1 << 2 | 1 << 13;

    public List<List<Vector3>> validWanderLocations { get; protected set; } = new List<List<Vector3>>();
    public float wanderRange = 25;
    protected float baseWanderRange = 25;

    public ActiveAttack(Enemy owner)
    {
        this.owner = owner;
        GameController.OnLevelChange += OnLevelChange;
        currentLevel = startingLevel;
        tree = new BehaviorTree.Tree();
    }

    public override void Update(float dt)
    {
        tree.UpdateTree(dt);
    }

    public void SetCurrentTarget(Transform t)
    {
        currentTargetDynamic = t;
        if(t != null)
        {
            currentTargetStatic = t.position;
        }
        else
            currentTargetStatic = Vector3.negativeInfinity;
    }
    public void SetCurrentTarget(Vector3 position)
    {
        currentTargetStatic = position;
    }

    protected void GetWanderLocations(Vector3 center, int sectorCount)
    {
        validWanderLocations.Clear();
        float diff = wanderRange / sectorCount;

        // Get a large array of valid points to choose from
        for (int i = 0; i < sectorCount; i++)
        {
            validWanderLocations.Add(new List<Vector3>());

            for (int j = 0; j < 360; j += 10)
            {
                float min = i * diff;
                min += i == 0 ? 2 : 0;
                float dist = Random.Range(min, min + diff);

                Vector3 point = new Vector3
                (
                    Mathf.Cos(j) * dist,
                    center.y,
                    Mathf.Sin(j) * dist
                );

                Ray ray = new Ray(point + center, Vector3.down);
                RaycastHit hit;

                

                // Check to see if the ray hit the ground
                if
                    (
                    Physics.Raycast(ray, out hit, 100, envLayers) &&
                    hit.collider.gameObject.layer != 13 &&
                    hit.normal == Vector3.up &&
                    !Physics.CheckBox(hit.point + Vector3.up * 2, Vector3.one, Quaternion.identity, envLayers)
                    )
                {
                    NavMeshPath path = new NavMeshPath();
                    owner.navAgent.CalculatePath(hit.point, path);

                    if(path.status == NavMeshPathStatus.PathComplete)
                    {
                        point = hit.point;

                        Debug.DrawRay(point, Vector3.up * 10, Color.green, 10f);
                        validWanderLocations[i].Add(point);
                    }
                    else
                    {
                        Debug.DrawRay(point, Vector3.up * 10, Color.red, 10f);
                    }
                }
            }
        }
    }
}
