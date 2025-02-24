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
    public List<EnemyNavNode> nodeQueue = new List<EnemyNavNode>();
    private EnemyNavNode previousNode;

    // These methods allow the enemy to update the values for attacks during level up
    private TaskWait n_AtNodePauseN;
    private TaskRushTarget n_RushTargetN;

    private TaskWait n_AtNodePauseI;
    private TaskRushTarget n_RushTargetI;

    //private TaskWait n_AtNodePause;
    //private TaskRushTarget n_RushTarget;

    private List<EnemyNavNode> visitedNodes = new List<EnemyNavNode>();

    public aa_Rush(Enemy owner) : base(owner)
    {
        name = "Rush";
        toolTip = "I couldn't even tell you if I wanted to";
    }

    public override void Initialize(int level = 1) 
    {
        base.Initialize(level);

        recentAudioSources.Add(null);

        owner.navAgent = owner.GetComponent<NavMeshAgent>();
        RefreshPath();

        // References stored so that they can have values changed later
        n_AtNodePauseN = new TaskWait(this, baseReachGoalPauseMin, baseReachGoalPauseMax);
        n_RushTargetN = new TaskRushTarget(this, owner.navAgent, baseAtNodePause, baseRushSpeed);

        n_AtNodePauseI = new TaskWait(this, baseReachGoalPauseMin, baseReachGoalPauseMax);
        n_RushTargetI = new TaskRushTarget(this, owner.navAgent, baseAtNodePause, baseRushSpeed);

        // Establises the Behavior Tree and its logic
        Node root = new Selector(new List<Node>()
        {
            // Attack any player that gets within range
            new Sequence(new List<Node>()
            {
                new CheckPlayerInRange(owner, 3),
                new TaskAttackPlayersInRange(owner.navAgent, 3)
            }),
            new Sequence(new List<Node>()
            {
                new CheckConditionAudioSourcePresent(this),
                new TaskRushBeginInvestigation(this),
                new Sequence(new List<Node>()
                {
                    n_RushTargetI,
                    n_AtNodePauseI
                }),
            }),
            new Sequence(new List<Node>
            {
                n_RushTargetN,
                new TaskRushGetNewPath(this),
                n_AtNodePauseN
            }),
        });

        tree.SetupTree(root);
    }

    public override void DetectSound(AudioSourceController.SourceData data)
    {
        recentAudioSources[0] = data;
        visitedNodes.Clear();
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

    public void RefreshPath()
    {
        if (nodeQueue.Count == 0)
        {
            // Start the process from the beginning

            nodeQueue.Add(EnemyNavGraph.GetRandomNavPoint());
            previousNode = EnemyNavGraph.GetFarthestNavPoint(nodeQueue[0].position);

            nodeQueue.Add(previousNode.GetRandomNeighbor(visitedNodes));

            // Warps the agent to where it is supposed to be for the first path
            owner.navAgent.Warp(nodeQueue[0].position);
        }
        else
        {
            GetNextPath();
        }

        path = EnemyNavGraph.GetPathToPoint(previousNode, nodeQueue[0]);

        for (int i = 0; i < path.Count - 1; i++)
        {
            path[i].RayToNode(path[i + 1]);
        }
    }
    private void GetNextPath()
    {
        visitedNodes.Add(nodeQueue[0]);

        previousNode = nodeQueue[0];
        nodeQueue.RemoveAt(0);

        // Check to see if a next node is already present, meaning there is more in the queue
        if (nodeQueue.Count > 1)
            return;

        EnemyNavNode tempNode = previousNode.GetRandomNeighbor(visitedNodes);

        // If valid node was found, continue with that, if not, start again
        if (tempNode != null)
            nodeQueue.Add(tempNode);
        else
        {
            visitedNodes.Clear();
            nodeQueue.Add(previousNode.GetRandomNeighbor(visitedNodes));
        }
    }

    protected override void OnLevelChange(int level)
    {
        base.OnLevelChange(level);

        n_AtNodePauseN.OnLevelChange(baseReachGoalPauseMin, baseReachGoalPauseMax);
        n_RushTargetN.OnLevelChange(baseAtNodePause, baseRushSpeed);
    }
}
