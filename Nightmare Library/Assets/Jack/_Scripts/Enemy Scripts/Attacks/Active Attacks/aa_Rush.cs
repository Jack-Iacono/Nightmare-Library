using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class aa_Rush : ActiveAttack
{
    private float preRushPause = 1;
    private float postRushPause = 3;
    private float sightAngle = -0.4f;

    public static readonly LayerMask envLayers = 1 << 9;

    public aa_Rush(Enemy owner) : base(owner)
    {

    }

    protected override Node SetupTree()
    {
        owner.navAgent = owner.GetComponent<NavMeshAgent>();

        // Establises the Behavior Tree and its logic
        Node root = new Selector(new List<Node>()
        {
            new TaskRushCooldown("RushCooldownTimer", postRushPause, owner),
            new Sequence(new List<Node>()
            {
                new CheckInAttackRange(owner),
                new TaskAttackPlayerQuick("Attacking Player Timer", 3, owner)
            }),
            new Sequence(new List<Node>
            {
                new CheckIsRushing(),
                new TaskRushTarget(owner.transform, owner.navAgent),
                new TaskStopRush(owner)
            }),
            new Sequence(new List<Node>
            {
                new CheckPlayerInSightRush(owner, owner.fovRange, sightAngle),
                new TaskWait("RushWait", preRushPause),
                new TaskStartRush(owner)
            }),
            new TaskPatrol(owner.transform, GameController.instance.patrolPoints, owner.navAgent)
        });

        root.SetData("speed", owner.moveSpeed);
        root.SetData("angularSpeed", owner.navAgent.angularSpeed);

        return root;
    }
}
