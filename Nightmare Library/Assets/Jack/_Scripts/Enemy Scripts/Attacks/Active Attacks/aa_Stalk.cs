using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class aa_Stalk : ActiveAttack
{
    private float sightAngle = -0.4f;

    protected int stalkAttemptMin = 2;
    protected int stalkAttemptMax = 4;
    public int stalkAttemptCounter = 0;

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
            // Attack Behavior
            new Sequence(new List<Node>()
            {
                new CheckTargetInRange(this, owner.transform, 4),
                new TaskAttackTarget(owner.navAgent),
                new TaskWait(2),
                new TaskResetStalk(this)
            }),
            // Run Away Behavior
            new Sequence(new List<Node>()
            {
                new CheckInPlayerSight(this, owner),
                new TaskWait(1),
                new TaskRunAway(owner.navAgent),
            }),
            // Close In Behavior
            new Sequence(new List<Node>()
            {
                new CheckTargetInRangeCloseIn(this, owner.navAgent, 10),
                new TaskWait(3),
                new TaskStalkCloseIn(this, owner.navAgent)
            }),
            // Approach Behavior
            new Sequence(new List<Node>()
            {
                new CheckConditionStalkCounter(this, owner.navAgent),
                new TaskWait(4),
                new TaskStalkApproach(this, owner.navAgent),
            }),
            // Wandering Behavior
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
        currentTarget = PlayerController.playerInstances[Random.Range(0, PlayerController.playerInstances.Count)].transform;
    }
    public void RemoveTarget()
    {
        currentTarget = null;
    }

    public void UseStalkAttempt()
    {
        stalkAttemptCounter--;
    }
    public void EmptyStalkAttempts()
    {
        stalkAttemptCounter = -1;
    }
}
