using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;
using UnityEngine.AI;

public class aa_Warden : ActiveAttack
{
    EnemyNavNode areaCenter = null;
    protected int mineCount = 2;
    protected int ringCount = 4;

    protected float diff;

    public aa_Warden(Enemy owner) : base(owner)
    {
        diff = wanderRange / ringCount;
    }

    protected override Node SetupTree()
    {
        owner.navAgent = owner.GetComponent<NavMeshAgent>();
        AssignArea();
        GetWanderLocations(areaCenter.position, ringCount);
        PlaceMines();

        // Establishes the Behavior Tree and its logic
        Node root = new Selector(new List<Node>()
        {
            new TaskWander(this, owner.navAgent)
        });

        return root;
    }

    protected void AssignArea()
    {
        areaCenter = EnemyNavGraph.GetRandomNavPoint();
        Debug.DrawRay(areaCenter.position, Vector3.up * 100, Color.yellow, 10f);
    }
    protected void PlaceMines()
    {
        for(int i = 0; i < mineCount * ringCount; i++)
        {
            int ring = Mathf.FloorToInt(i / mineCount);
            Vector3 pos = validWanderLocations[ring][Random.Range(0, validWanderLocations[ring].Count)];

            Debug.DrawRay(pos, Vector3.up * 100, UnityEngine.Color.cyan, 10f);
            SpawnMine(pos);
        }
    }
    
    protected void SpawnMine(Vector3 pos)
    {

    }
}
