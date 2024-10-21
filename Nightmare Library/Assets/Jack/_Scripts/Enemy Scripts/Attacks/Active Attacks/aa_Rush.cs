using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;
using UnityEngine.AI;

public class aa_Rush : ActiveAttack
{
    private float reachGoalPause = 5;
    private float atNodePause = 0.5f;
    private float rushSpeed = 50;

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
            new Sequence(new List<Node>()
            {
                new CheckPlayerInRange(owner, 3),
                new TaskAttackPlayersInRange(owner.navAgent, 3)
            }),
            new Sequence(new List<Node>
            {
                new TaskRushTarget(this, owner.navAgent, atNodePause, rushSpeed),
                new TaskWait(reachGoalPause)
            }),
        });

        root.SetData("speed", owner.moveSpeed);
        root.SetData("angularSpeed", owner.navAgent.angularSpeed);

        return root;
    }
}
