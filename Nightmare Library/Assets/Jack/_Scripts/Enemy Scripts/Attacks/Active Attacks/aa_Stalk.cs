using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class aa_Stalk : ActiveAttack
{
    private float sightAngle = -0.4f;
    public PlayerController currentTargetPlayer;

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
            // Handle Players Entering Office Area
            new Sequence(new List<Node>()
            {
                // Check if the target player is at the desk
                new CheckStalkTargetInMap(this),
                new Selector(new List<Node>()
                {
                    // Attempt to assign a new target
                    new TaskAssignStalkTarget(this),
                    // End the stalk phase due to lack of players
                    new TaskResetStalk(this)
                })
            }),
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
                new TaskWait(0.5f),
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
                new TaskAssignStalkTarget(this)
            })
        });

        root.SetData("speed", owner.moveSpeed);
        root.SetData("angularSpeed", owner.navAgent.angularSpeed);

        return root;
    }

    public bool BeginStalking()
    {
        if(DeskController.playersAtDesk.Count < PlayerController.playerInstances.Count)
        {
            // Set the amount of stalking attempts this attack will have
            stalkAttemptCounter = Random.Range(stalkAttemptMin, stalkAttemptMax + 1);

            // Remove any players that are at the desk
            List<PlayerController> validPlayers = new List<PlayerController>(PlayerController.playerInstances);
            foreach(PlayerController p in DeskController.playersAtDesk)
            {
                validPlayers.Remove(p);
            }

            // Set the stalking target for this attack
            int rand = Random.Range(0, validPlayers.Count);
            currentTargetPlayer = validPlayers[rand];
            SetCurrentTarget(currentTargetPlayer.transform);

            return true;
        }

        return false;
    }
    public void RemoveTarget()
    {
        SetCurrentTarget(null);
        currentTargetPlayer = null;
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
