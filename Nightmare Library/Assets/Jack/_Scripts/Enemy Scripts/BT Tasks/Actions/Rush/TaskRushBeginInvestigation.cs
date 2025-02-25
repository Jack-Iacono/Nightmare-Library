using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class TaskRushBeginInvestigation : Node
{
    private bool hasRun = false;
    private aa_Rush owner;

    private int passSetCount = 2;

    public TaskRushBeginInvestigation(aa_Rush owner)
    {
        this.owner = owner;
    }

    public override Status Check(float dt)
    {
        if (!hasRun)
        {
            EnemyNavGraph.NeighborPair pair = EnemyNavGraph.GetClosestNodePair(owner.recentAudioSources[0].position);

            // Makes sure that the enemy has to move to the first node
            bool normalNodeSet = owner.nodeQueue[0] != pair.node1;
            EnemyNavNode current = owner.nodeQueue[0];
            owner.nodeQueue.Clear();

            for (int i = 0; i < passSetCount; i++)
            {
                if (normalNodeSet)
                {
                    owner.nodeQueue.Add(pair.node1);
                    owner.nodeQueue.Add(pair.node2);
                }
                else
                {
                    owner.nodeQueue.Add(pair.node2);
                    owner.nodeQueue.Add(pair.node1);
                }
            }

            owner.path = EnemyNavGraph.GetPathToPoint(owner.previousNode, owner.nodeQueue[0]);
            hasRun = true;
        }

        return Status.SUCCESS;
    }
    protected override void OnResetNode()
    {
        hasRun = false;
        base.OnResetNode();
    }
}
