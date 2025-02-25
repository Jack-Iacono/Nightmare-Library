using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class TaskRushResetQueue : Node
{
    private aa_Rush owner;

    public TaskRushResetQueue(aa_Rush owner)
    {
        this.owner = owner;
    }

    public override Status Check(float dt)
    {
        owner.recentAudioSources.RemoveAt(0);

        owner.visitedNodes.Clear();
        owner.nodeQueue.Add(EnemyNavGraph.GetFarthestNavPoint(owner.previousNode.position));
        owner.nodeQueue.Add(owner.previousNode.GetRandomNeighbor(null));

        return Status.SUCCESS;
    }
}
