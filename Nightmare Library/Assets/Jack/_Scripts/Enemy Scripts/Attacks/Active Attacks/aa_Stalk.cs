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

    // How quickly will the enemy gett irritated and begin a stalking phase
    protected int irritationThreshold = 5;
    protected int irritationCounter = 0;

    // How long should the enemy wait after being seen and running away
    protected float baseRunTimeMin = 3;
    protected float baseRunTimeMax = 7;

    // Parameters for the player seeing this enemy
    protected float playerSightRange = 50;
    protected float playerSightAngle = 0.6f;

    protected Action_WanderTimed n_WanderTime;
    protected Action_StalkCloseIn n_CloseIn;
    protected Action_Wait n_AttackWait;
    protected Action_Wait n_RunAwayWait;

    protected Check_Counter n_StalkCounter;

    public aa_Stalk(Enemy owner) : base(owner)
    {
        name = "Stalker";
        toolTip = "This guy should go to jail, clearly what they do isn't legal. Especially the killing part, that might be bad.";
        hearingRadius = 10;
    }

    public override void Initialize(int level = 1)
    {
        base.Initialize(level);

        owner.navAgent = owner.GetComponent<NavMeshAgent>();

        n_AttackWait = new Action_Wait(baseAttackTimeWaitMin, baseAttackTimeWaitMax);
        n_CloseIn = new Action_StalkCloseIn(this, owner.navAgent);
        n_WanderTime = new Action_WanderTimed(this, owner.navAgent, baseWanderTimeMin, baseWanderTimeMax);
        n_RunAwayWait = new Action_Wait(baseRunTimeMin, baseRunTimeMax);

        n_StalkCounter = new Check_Counter(0, Check_Counter.EvalType.GREATER);

        // Establises the Behavior Tree and its logic
        Node root = new Selector(new List<Node>()
        {
            // Handle Players Entering Office Area (this goes before just in case the target is set while not attacking
            new Sequence(new List<Node>()
            {
                // Check if the target player is at the desk
                new Check_ConditionStalkTargetOutOffice(this),
                new Selector(new List<Node>()
                {
                    // Attempt to assign a new target
                    new Action_StalkAssignTarget(this),
                    // End the stalk phase due to lack of players
                    new Action_StalkReset(this)
                })
            }),
            new Sequence(new List<Node>()
            {
                n_StalkCounter,
                new Selector(new List<Node>()
                {
                    // Run Away Behavior
                    new Sequence(new List<Node>()
                    {
                        new Check_InPlayerSight(this, owner, playerSightRange, playerSightAngle),
                        new Action_Wait(0.25f),
                        new Action_WarpAway(this,owner.navAgent),
                        new Action_CounterChange(n_StalkCounter, Action_CounterChange.ChangeType.SUBTRACT, 1)
                    }),
                    // Attack Behavior
                    new Sequence(new List<Node>()
                    {
                        new Check_TargetInRange(this, owner.transform, 4),
                        new Action_StalkAttackTarget(owner, this),
                        new Action_Wait(3),
                        new Action_StalkReset(this),
                        new Action_CounterChange(n_StalkCounter, Action_CounterChange.ChangeType.SET, -1),
                        new Action_WarpAway(this,owner.navAgent),
                    }),
                    // Warp behind and approach
                    new Sequence(new List<Node>()
                    {
                        new Action_WarpBehindPlayer(owner, GetCurrentTarget),
                        new Action_PlaySound(AudioManager.SoundType.e_STALK_CLOSE_IN, GetCurrentTargetPosition),
                        n_AttackWait,
                        n_CloseIn
                    })
                }),
            }),
            // Wandering Behavior
            new Sequence(new List<Node>()
            {
                n_WanderTime,
                new Action_StalkAssignTarget(this)
            })
        });

        tree.SetupTree(root);
    }

    public override bool DetectSound(AudioSourceController.SourceData data)
    {
        if (base.DetectSound(data))
        {
            // Always raise the irritation count on any sound
            if (irritationCounter < irritationThreshold)
            {
                irritationCounter++;
            }
            else if (PlayerController.playerInstances.ContainsKey(data.gameObject))
            {
                PlayerController cont = PlayerController.playerInstances[data.gameObject];
                currentTargetPlayer = cont;
                BeginStalking();
                irritationCounter = 0;

                // Makes the enemy wander after this attack for the correct time
                n_WanderTime.ResetTime();
            }

            return true;
        }

        return false;
    }

    public bool BeginStalking()
    {
        if(DeskController.playersAtDesk.Count < PlayerController.playerInstances.Count)
        {
            // Set the amount of stalking attempts this attack will have
            n_StalkCounter.Set(Random.Range(currentStalkAttemptMin, currentStalkAttemptMax + 1));

            // If the enemy already has a target, allow it to attack that one without reassigning
            if(currentTargetPlayer == null)
            {
                // Remove any players that are at the desk
                List<PlayerController> validPlayers = new List<PlayerController>(PlayerController.playerInstances.Values);
                for (int i = validPlayers.Count - 1; i > 0; i--)
                {
                    if (!validPlayers[i].isAlive || DeskController.playersAtDesk.Contains(validPlayers[i]))
                        validPlayers.Remove(validPlayers[i]);
                }

                // Set the stalking target for this attack
                currentTargetPlayer = validPlayers[Random.Range(0, validPlayers.Count)];
            }
            
            SetCurrentTarget(currentTargetPlayer.transform);

            return true;
        }

        currentTargetPlayer = null;
        return false;
    }

    public void AssignTarget(PlayerController player)
    {
        // Deciding what to do here

    }
    public void RemoveTarget()
    {
        SetCurrentTarget(null);
        currentTargetPlayer = null;
    }

    public PlayerController GetCurrentTarget()
    {
        return currentTargetPlayer;
    }
    public Vector3 GetCurrentTargetPosition()
    {
        if(currentTargetPlayer != null)
            return currentTargetPlayer.transform.position;
        else
            return owner.transform.position;
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
