using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using static AudioManager;

public class aa_Rush : ActiveAttack
{
    private float baseReachGoalPauseMin = 5;
    private float baseReachGoalPauseMax = 5;
    private float baseAtNodePause = 0.1f;
    private float baseRushSpeed = 150;

    // These methods allow the enemy to update the values for attacks during level up
    private Node_Wait n_AtNodePauseN;
    private Node_PathTraverse n_RushTargetN;

    private Node_Wait n_AtNodePauseI;
    private Node_PathTraverse n_RushTargetI;

    private Node_CheckCounter n_PathCompleteCounter;

    public List<EnemyNavNode> visitedNodes = new List<EnemyNavNode>();

    // TEMPORARY so that I can save quick
    public EnemyNavNode currentNode;
    public List<EnemyNavNode> nodeQueue;
    public void SetNodeQueue(List<EnemyNavNode> nodeQueue)
    {

    }

    public aa_Rush(Enemy owner) : base(owner)
    {
        name = "Rush";
        ignoreSounds = new SoundType[]{ SoundType.e_STALK_CLOSE_IN };
        toolTip = "I couldn't even tell you if I wanted to, it just seems mad";
        hearingRadius = 100;
    }

    public override void Initialize(int level = 1) 
    {
        base.Initialize(level);

        owner.navAgent = owner.GetComponent<NavMeshAgent>();

        PathNodeData normalPathData = new PathNodeData(owner);
        PathNodeData investigatePathData = new PathNodeData(owner);

        // References stored so that they can have values changed later
        n_AtNodePauseN = new Node_Wait(baseReachGoalPauseMin, baseReachGoalPauseMax);
        n_RushTargetN = new Node_PathTraverse(normalPathData, baseAtNodePause, baseRushSpeed);

        n_AtNodePauseI = new Node_Wait(0.1f, 0.5f);
        n_RushTargetI = new Node_PathTraverse(investigatePathData, baseAtNodePause, baseRushSpeed);

        n_PathCompleteCounter = new Node_CheckCounter(1, Node_CheckCounter.EvalType.GREATER_EQUAL);

        

        // Establises the Behavior Tree and its logic
        Node root = new Selector(new List<Node>()
        {
            // Attack any player that gets within range
            new Sequence(new List<Node>()
            {
                new Node_CheckInPlayerRange(owner, 3),
                new Node_AttackInRange(owner, 3)
            }),
            // Area patrol from hearing noise
            new Sequence(new List<Node>()
            {
                new Node_CheckAudioSourcePresent(this),
                new Node_RushBeginInvestigation(this),
                new Selector(new List<Node>()
                {
                    new Sequence(new List<Node>()
                    {
                        new Node_CheckRushInvestigationEnd(this),
                        new Node_Wait(1),
                        new Node_RushEndInvestigation(this)
                    }),
                    new Sequence(new List<Node>()
                    {
                        new Node_PathSet(investigatePathData),
                        n_RushTargetI,
                        n_AtNodePauseI,
                        new Node_PathComplete(investigatePathData, false),
                        new Node_Running()
                    })
                }),
            }),
            new Selector(new List<Node>()
            {
                // Pause after completing a certain number of paths (Not including paths from investigation)
                new Sequence(new List<Node>()
                {
                    n_PathCompleteCounter,
                    new Node_Wait(9),
                    new Node_PlaySound(AudioManager.SoundType.e_STALK_CLOSE_IN, owner.transform),
                    new Node_Wait(2),
                    new Node_ChangeCounter(n_PathCompleteCounter, Node_ChangeCounter.ChangeType.RESET)
                }),
                new Sequence(new List<Node>
                {
                    new Node_PathSet(normalPathData),
                    n_RushTargetN,
                    n_AtNodePauseN,
                    new Node_PathComplete(normalPathData, false),
                    new Node_ChangeCounter(n_PathCompleteCounter, Node_ChangeCounter.ChangeType.ADD, 1)
                }),
            })
        });

        tree.SetupTree(root);
    }

    public override bool DetectSound(AudioSourceController.SourceData data)
    {
        if (base.DetectSound(data))
        {
            if (recentAudioSources.Count < 3)
                recentAudioSources.Add(data);
            visitedNodes.Clear();

            return true;
        }

        return false;
    }
    protected override void OnLevelChange(int level)
    {
        base.OnLevelChange(level);

        n_AtNodePauseN.OnLevelChange(baseReachGoalPauseMin, baseReachGoalPauseMax);
        //n_RushTargetN.OnLevelChange(baseAtNodePause, baseRushSpeed);
    }
}
