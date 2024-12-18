using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class aa_Rush : ActiveAttack
{
    private float reachGoalPause = 5;
    private float atNodePause = 0.1f;
    private float rushSpeed = 150;

    public List<EnemyNavNode> path { get; private set; } = new List<EnemyNavNode>();
    private EnemyNavNode currentGoal;
    private EnemyNavNode previousGoal;

    public aa_Rush(Enemy owner) : base(owner)
    {
    }

    protected override Node SetupTree()
    {
        owner.navAgent = owner.GetComponent<NavMeshAgent>();
        GetNewPath();

        // Establises the Behavior Tree and its logic
        Node root = new Selector(new List<Node>()
        {
            // Attack any player that gets within range
            new Sequence(new List<Node>()
            {
                new CheckPlayerInRange(owner, 3),
                new TaskAttackPlayersInRange(owner.navAgent, 3)
            }),
            new Sequence(new List<Node>
            {
                new TaskRushTarget(this, owner.navAgent, atNodePause, rushSpeed),
                new TaskWait(reachGoalPause),
                new TaskRushGetNewPath(this)
            }),
        });

        return root;
    }

    public EnemyNavNode GetNextNode()
    {
        
        if(path.Count > 0)
        {
            Debug.Log("Getting Next Node: " + path[0].name);

            EnemyNavNode node = path[0];
            path.RemoveAt(0);
            return node;
        }
        return null;
    }
    public void GetNewPath()
    {
        if (currentGoal == null)
            currentGoal = EnemyNavGraph.GetClosestNavPoint(owner.transform.position);

        previousGoal = currentGoal;
        currentGoal = EnemyNavGraph.GetRandomNavPoint();

        path = EnemyNavGraph.GetPathToPoint(previousGoal, currentGoal);

        ///*
        for(int i = 0; i < path.Count - 1; i++)
        {
            path[i].RayToNode(path[i + 1]);
        }
        //*/
    }
}
