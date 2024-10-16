using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class aa_Rush : ActiveAttack
{
    public bool isRushing = false;

    private float preRushPause = 1;
    private float postRushPause = 3;
    private float sightAngle = -0.4f;

    public aa_Rush(Enemy owner) : base(owner)
    {
        
    }

    protected override Node SetupTree()
    {
        owner.navAgent = owner.GetComponent<NavMeshAgent>();

        // Establises the Behavior Tree and its logic
        Node root = new Selector(new List<Node>()
        {
            // Attack any player that gets within range
            new TaskAttackPlayersInRange(owner.navAgent, 3),
            new Sequence(new List<Node>
            {
                new CheckIsRushing(this),
                new TaskRushTarget(this, owner.navAgent),
                new TaskWait(postRushPause),
                new TaskStopRush(this)
            }),
            new Sequence(new List<Node>
            {
                new CheckPlayerInSight(this, owner.navAgent, owner.fovRange, sightAngle),
                new TaskWait(preRushPause),
                new TaskStartRush(this)
            }),
            new TaskPatrol(owner.transform, EnemyNavPointController.enemyNavPoints, owner.navAgent)
        });

        root.SetData("speed", owner.moveSpeed);
        root.SetData("angularSpeed", owner.navAgent.angularSpeed);

        return root;
    }
}
