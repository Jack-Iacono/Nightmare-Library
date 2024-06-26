using BehaviorTree;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemy : Enemy
{
    protected BasicEnemyBT behaviorTree;

    private void Start()
    {
        base.Initialize();

        behaviorTree = new BasicEnemyBT(this);
        behaviorTree.Initialize();
        currentTree = behaviorTree;
    }

    protected override void Update()
    {
        float dt = Time.deltaTime;

        currentTree.UpdateTree(dt);
    }
}
