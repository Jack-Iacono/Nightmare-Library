using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class aa_Stalk : ActiveAttack
{
    //private float sightAngle = -0.4f;
    public PlayerController currentTargetPlayer;

    protected const int baseStalkAttemptMin = 2;
    protected const int baseStalkAttemptMax = 4;
    protected int currentStalkAttemptMin = baseStalkAttemptMin;
    protected int currentStalkAttemptMax = baseStalkAttemptMax;
    public int stalkAttemptCounter = 0;

    // How long should the enemy remain in it's wander stage
    protected float baseWanderTimeMin = 10;
    protected float baseWanderTimeMax = 20;

    // How long should the stalker wait before attacking
    protected float baseAttackTimeWaitMin = 4;
    protected float baseAttackTimeWaitMax = 7;

    // How fast should the stalker move when closing in
    protected float baseCloseInSpeed = 20;
    protected float baseCloseInAccel = 400;

    // How long should the enemy wait after being seen and running away
    protected float baseRunTimeMin = 3;
    protected float baseRunTimeMax = 7;

    protected TaskWanderTimed n_WanderTime;
    protected TaskStalkCloseIn n_CloseIn;
    protected TaskWait n_AttackWait;
    protected TaskWait n_RunAwayWait;

    public aa_Stalk(Enemy owner) : base(owner)
    {
        name = "Stalker";
        toolTip = "This guy should go to jail, clearly what they do isn't legal. Especially the killing part, that might be bad.";
    }

    public override void Initialize(int level = 1)
    {
        base.Initialize(level);

        owner.navAgent = owner.GetComponent<NavMeshAgent>();

        n_AttackWait = new TaskWait(baseAttackTimeWaitMin, baseAttackTimeWaitMax);
        n_CloseIn = new TaskStalkCloseIn(this, owner.navAgent);
        n_WanderTime = new TaskWanderTimed(this, owner.navAgent, baseWanderTimeMin, baseWanderTimeMax);
        n_RunAwayWait = new TaskWait(baseRunTimeMin, baseRunTimeMax);

        // Establises the Behavior Tree and its logic
        Node root = new Selector(new List<Node>()
        {
            // Handle Players Entering Office Area (this goes before just in case the target is set while not attacking
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
            new Sequence(new List<Node>()
            {
                new CheckConditionStalkCounter(this, owner.navAgent),
                new Selector(new List<Node>()
                {
                    // Run Away Behavior
                    new Sequence(new List<Node>()
                    {
                        new CheckInPlayerSight(this, owner),
                        new TaskWait(0.25f),
                        new TaskWarpAway(this,owner.navAgent),
                        n_RunAwayWait
                    }),
                    // Attack Behavior
                    new Sequence(new List<Node>()
                    {
                        new CheckTargetInRange(this, owner.transform, 4),
                        new TaskAttackTarget(owner.navAgent),
                        new TaskWait(3),
                        new TaskResetStalk(this),
                        new TaskWarpAway(this,owner.navAgent),
                    }),
                    // Warp behind and approach
                    new Sequence(new List<Node>()
                    {
                        new TaskStalkWarpBehind(this, owner),
                        n_AttackWait,
                        n_CloseIn
                    })
                }),
            }),
            // Wandering Behavior
            new Sequence(new List<Node>()
            {
                n_WanderTime,
                new TaskAssignStalkTarget(this)
            })
        });

        tree.SetupTree(root);
    }

    public bool BeginStalking()
    {
        if(DeskController.playersAtDesk.Count < PlayerController.playerInstances.Count)
        {
            // Set the amount of stalking attempts this attack will have
            stalkAttemptCounter = Random.Range(currentStalkAttemptMin, currentStalkAttemptMax + 1);

            // Remove any players that are at the desk
            List<PlayerController> validPlayers = new List<PlayerController>(PlayerController.playerInstances.Values);
            for(int i = validPlayers.Count - 1; i > 0; i--)
            {
                if (!validPlayers[i].isAlive || DeskController.playersAtDesk.Contains(validPlayers[i]))
                    validPlayers.Remove(validPlayers[i]);
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

    protected override void OnLevelChange(int level)
    {
        base.OnLevelChange(level);

        currentStalkAttemptMin = baseStalkAttemptMin + Mathf.FloorToInt(level / 2);
        currentStalkAttemptMax = baseStalkAttemptMax + level;

        n_WanderTime.OnLevelChange(baseWanderTimeMin, baseWanderTimeMax);
        n_AttackWait.OnLevelChange(baseAttackTimeWaitMin, baseAttackTimeWaitMax);
        n_CloseIn.OnLevelChange(baseCloseInSpeed, baseCloseInAccel);
        n_RunAwayWait.OnLevelChange(baseRunTimeMin, baseRunTimeMax);
    }
}
