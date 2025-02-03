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

    // These methods allow the enemy to update the values for attacks during level up
    private TaskWait atNodePauseNode;
    private TaskRushTarget rushTargetNode;

    public aa_Rush(Enemy owner) : base(owner)
    {
        name = "Rush";
        toolTip = "I couldn't even tell you if I wanted to";
    }

    public override void Initialize() 
    {
        owner.navAgent = owner.GetComponent<NavMeshAgent>();
        GetNewPath();

        // References stored so that they can have values changed later
        atNodePauseNode = new TaskWait(this, reachGoalPause, 0);
        rushTargetNode = new TaskRushTarget(this, owner.navAgent, atNodePause, rushSpeed);

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
                rushTargetNode,
                atNodePauseNode,
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

        atNodePauseNode.waitTime = Mathf.Clamp(reachGoalPause - (level * 0.5f), 0, 1000000);

        rushTargetNode.nodeWaitTime = atNodePause;
        rushTargetNode.speed = rushSpeed * level;
    }
}
