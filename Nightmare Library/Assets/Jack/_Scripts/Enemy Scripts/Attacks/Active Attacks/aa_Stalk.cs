using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class aa_Stalk : ActiveAttack
{
    private float sightAngle = -0.4f;

    private int stalkAttemptMin = 2;
    private int stalkAttemptMax = 4;
    public int stalkAttemptCounter = 0;

    public PlayerController currentTarget { get; private set; } = null;

    public static readonly LayerMask envLayers = 1 << 9 | 1 << 2;

    public aa_Stalk(Enemy owner) : base(owner)
    {

    }

    protected override Node SetupTree()
    {
        owner.navAgent = owner.GetComponent<NavMeshAgent>();

        // Establises the Behavior Tree and its logic
        Node root = new Selector(new List<Node>()
        {
            new Sequence(new List<Node>()
            {
                new CheckInPlayerSight(this, owner),
                new TaskWait(1),
                new TaskRunAway(owner.navAgent),
            }),
            new Sequence(new List<Node>()
            {
                new CheckStalkCloseIn(this, owner.navAgent, 10),
                new TaskMakeNoise(),
                new TaskWait(3),
                new TaskStalkCloseIn(this, owner.navAgent)
            }),
            new TaskStalkApproach(this, owner.navAgent),
            new Sequence(new List<Node>()
            {
                new TaskTimedWander(this, owner.navAgent),
                new TaskStartStalking(this)
            })
        });

        root.SetData("speed", owner.moveSpeed);
        root.SetData("angularSpeed", owner.navAgent.angularSpeed);

        return root;
    }

    public void BeginStalking()
    {
        // Set the amount of stalking attempts this attack will have
        stalkAttemptCounter = Random.Range(stalkAttemptMin, stalkAttemptMax + 1);
        // Set the stalking target for this attack
        currentTarget = PlayerController.playerInstances[Random.Range(0, PlayerController.playerInstances.Count)];
    }
    public void RemoveTarget()
    {
        currentTarget = null;
    }
    public void UseStalkAttempt()
    {
        stalkAttemptCounter--;
    }
}
