using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class aa_Rush : ActiveAttack
{
    private float baseReachGoalPauseMin = 5;
    private float baseReachGoalPauseMax = 5;
    private float baseAtNodePause = 0.1f;
    private float baseRushSpeed = 150;

    public List<EnemyNavNode> path { get; private set; } = new List<EnemyNavNode>();
    private EnemyNavNode currentGoal;
    private EnemyNavNode previousGoal;

    // These methods allow the enemy to update the values for attacks during level up
    private TaskWait n_AtNodePause;
    private TaskRushTarget n_RushTarget;

    public aa_Rush(Enemy owner) : base(owner)
    {
        name = "Rush";
        toolTip = "I couldn't even tell you if I wanted to";
    }

    public override void Initialize(int level = 1) 
    {
        base.Initialize(level);

        owner.navAgent = owner.GetComponent<NavMeshAgent>();
        GetNewPath();

        // References stored so that they can have values changed later
        n_AtNodePause = new TaskWait(this, baseReachGoalPauseMin, baseReachGoalPauseMax);
        n_RushTarget = new TaskRushTarget(this, owner.navAgent, baseAtNodePause, baseRushSpeed);

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
                n_RushTarget,
                n_AtNodePause,
                new TaskRushGetNewPath(this)
            }),
        });

        tree.SetupTree(root);
    }

    public EnemyNavNode GetNextNode()
    {
        
        if(path.Count > 0)
        {
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

        for(int i = 0; i < path.Count - 1; i++)
        {
            path[i].RayToNode(path[i + 1]);
        }
    }

    protected override void OnLevelChange(int level)
    {
        base.OnLevelChange(level);

        n_AtNodePause.OnLevelChange(baseReachGoalPauseMin, baseReachGoalPauseMax);
        n_RushTarget.OnLevelChange(baseAtNodePause, baseRushSpeed * level);
    }
}
