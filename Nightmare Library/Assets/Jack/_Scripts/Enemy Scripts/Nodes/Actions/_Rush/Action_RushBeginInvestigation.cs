using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class Action_RushBeginInvestigation : Node
{
    private bool hasRun = false;
    private aa_Rush owner;

    private int passSetCount = 2;

    public Action_RushBeginInvestigation(aa_Rush owner)
    {
        this.owner = owner;
    }

    public override Status Check(float dt)
    {
        if (!hasRun)
        {
            EnemyNavNode[] pair = EnemyNavGraph.GetClosestNodePair(owner.GetAudioSource(0).transform.position);

            // Makes sure that the enemy has to move to the first node
            bool normalNodeSet = owner.currentNode != pair[0];
            List<EnemyNavNode> newNodes = new List<EnemyNavNode>();

            // Lets the agent finish their current path
            newNodes.Add(owner.nodeQueue[0]);

            // Add the several pairs of nodes for the rush to run between
            for (int i = 0; i < passSetCount; i++)
            {
                if (normalNodeSet)
                {
                    newNodes.Add(pair[0]);
                    newNodes.Add(pair[1]);
                }
                else
                {
                    newNodes.Add(pair[1]);
                    newNodes.Add(pair[0]);
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
