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

    public List<EnemyNavNode> path = new List<EnemyNavNode>();
    public List<EnemyNavNode> nodeQueue { get; protected set; } = new List<EnemyNavNode>();
    public EnemyNavNode currentNode { get; protected set; }

    // These methods allow the enemy to update the values for attacks during level up
    private TaskWait n_AtNodePauseN;
    private TaskRushTarget n_RushTargetN;

    private TaskWait n_AtNodePauseI;
    private TaskRushTarget n_RushTargetI;

    public List<EnemyNavNode> visitedNodes = new List<EnemyNavNode>();

    public aa_Rush(Enemy owner) : base(owner)
    {
        name = "Rush";
        toolTip = "I couldn't even tell you if I wanted to";
    }

    public override void Initialize(int level = 1) 
    {
        base.Initialize(level);

        // Why is this here?
        SetCurrentNode(EnemyNavGraph.GetRandomNavPoint());
        RestartNodeQueue();
        SetCurrentPath();

        owner.navAgent = owner.GetComponent<NavMeshAgent>();
        owner.navAgent.Warp(currentNode.position);

        // References stored so that they can have values changed later
        n_AtNodePauseN = new TaskWait(baseReachGoalPauseMin, baseReachGoalPauseMax);
        n_RushTargetN = new TaskRushTarget(this, owner.navAgent, baseAtNodePause, baseRushSpeed);

        n_AtNodePauseI = new TaskWait(1, 1);
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
                new Selector(new List<Node>()
                {
                    new Sequence(new List<Node>()
                    {
                        new CheckConditionRushInvestigationEnd(this),
                        new TaskWait(5),
                        new TaskRushEndInvestigation(this)
                    }),
                    new Sequence(new List<Node>()
                    {
                        n_RushTargetI,
                        n_AtNodePauseI,
                        new TaskRushPathComplete(this, false),
                        new TaskRunning()
                    })
                }),
            }),
            new Sequence(new List<Node>
            {
                n_RushTargetN,
                n_AtNodePauseN,
                new TaskRushPathComplete(this, true)
            }),
        });

        tree.SetupTree(root);
    }

    public override void DetectSound(AudioSourceController.SourceData data)
    {
        if(recentAudioSources.Count < 3)
            recentAudioSources.Add(data);
        visitedNodes.Clear();
    }

    public EnemyNavNode GetNextPathNode()
    {
        if(path.Count > 0)
        {
            EnemyNavNode node = path[0];
            path.RemoveAt(0);
            return node;
        }
        return null;
    }

    /// <summary>
    /// Readys the nodeQueue and currentNode for the next path
    /// </summary>
    public void PathComplete(bool getNewNodes = true)
    {
        SetCurrentNode(nodeQueue[0]);
        nodeQueue.RemoveAt(0);

        if (nodeQueue.Count > 0)
        {
            if (getNewNodes)
            {
                EnemyNavNode tempNode = currentNode.GetRandomNeighbor(visitedNodes);

                // If valid node was found, continue with that, if not, clear the visited nodes and pick again
                if (tempNode != null)
                {
                    nodeQueue.Add(tempNode);
                }
                else
                {
                    RestartNodeQueue();
                }
            }
        }
        else
        {
            // Run as a fresh start with a random far patrol point
            RestartNodeQueue();
        }

        // Get the new path
        SetCurrentPath();
    }
    /// <summary>
    /// Sets the path to the one given and adjusts the path accordingly
    /// </summary>
    /// <param name="nodeQueue"></param>
    public void SetNodeQueue(List<EnemyNavNode> nodeQueue)
    {
        this.nodeQueue = new List<EnemyNavNode>(nodeQueue);
        visitedNodes.Clear();
    }
    
    /// <summary>
    /// Sets the current node as visited and assigns the current node to the value
    /// </summary>
    /// <param name="node"></param>
    private void SetCurrentNode(EnemyNavNode node)
    {
        currentNode = node;
        visitedNodes.Add(currentNode);
    }
    /// <summary>
    /// Gets an all new set of nodes based on the current node
    /// </summary>
    public void RestartNodeQueue()
    {
        visitedNodes.Clear();
        nodeQueue.Clear();

        nodeQueue.Add(EnemyNavGraph.GetFarthestNavPoint(currentNode.position));
        nodeQueue.Add(currentNode.GetRandomNeighbor(null));
    }
    
    /// <summary>
    /// Sets the current path based on the node and nodeQueue
    /// </summary>
    private void SetCurrentPath()
    {
        string s = string.Empty;

        Debug.Log(currentNode.name + " -> " + (nodeQueue.Count > 0 ? nodeQueue[0].name : "null"));

        // Get the path that the enemy will now follow
        path = EnemyNavGraph.GetPathToPoint(currentNode, nodeQueue[0]);

        // For in editor visual
        for (int i = 0; i < path.Count - 1; i++)
        {
            path[i].RayToNode(path[i + 1]);
        }
    }

    protected override void OnLevelChange(int level)
    {
        base.OnLevelChange(level);

        n_AtNodePauseN.OnLevelChange(baseReachGoalPauseMin, baseReachGoalPauseMax);
        n_RushTargetN.OnLevelChange(baseAtNodePause, baseRushSpeed);
    }
}
