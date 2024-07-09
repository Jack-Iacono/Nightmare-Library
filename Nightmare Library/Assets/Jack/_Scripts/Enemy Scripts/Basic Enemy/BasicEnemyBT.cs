using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;
using System;
using UnityEngine.AI;

public class BasicEnemyBT : BehaviorTree.Tree
{
    private BasicEnemy owner;

    public BasicEnemyBT(BasicEnemy owner)
    {
        this.owner = owner;
    }

    protected override Node SetupTree()
    {
        owner.navAgent = owner.GetComponent<NavMeshAgent>();

        // Establises the Behavior Tree and its logic
        Node root = new Selector(new List<Node>()
        {
            new Sequence(new List<Node>
            {
                new CheckPlayerInSightChase(owner, owner.fovRange, -0.4f),
                new TaskChasePlayer(owner.transform, owner.navAgent),
                new CheckArea(owner, PlayerController.playerInstances[0].gameObject.transform)
            }),
            new TaskPatrol(owner.transform, GameController.instance.patrolPoints, owner.navAgent)
        });

        root.SetData("speed", owner.moveSpeed);

        return root;
    }

    public void SetValue(string key, object value)
    {
        root.SetData(key, value);
    }
}
