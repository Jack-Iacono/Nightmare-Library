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
            EnemyNavGraph.NeighborPair pair = EnemyNavGraph.GetClosestNodePair(owner.GetAudioSource(0).transform.position);

            // Makes sure that the enemy has to move to the first node
            bool normalNodeSet = owner.currentNode != pair.node1;
            List<EnemyNavNode> newNodes = new List<EnemyNavNode>();

            // Lets the agent finish their current path
            newNodes.Add(owner.nodeQueue[0]);

            // Add the several pairs of nodes for the rush to run between
            for (int i = 0; i < passSetCount; i++)
            {
                if (normalNodeSet)
                {
                    newNodes.Add(pair.node1);
                    newNodes.Add(pair.node2);
                }
                else
                {
                    newNodes.Add(pair.node2);
                    newNodes.Add(pair.node1);
                }
            }

            // Get the new path
            owner.SetNodeQueue(newNodes);
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
