using BehaviorTree;
using UnityEngine;

public class Action_RushPathComplete : Node
{
    private aa_Rush owner;
    private bool getNodeOnComplete;

    /// <summary>
    /// Tells the Rush attack that the current path has completed
    /// </summary>
    /// <param name="owner">The Rush attack that owns this node</param>
    /// <param name="getNodeOnComplete">Should the rush attack add nodes to the queue on completion</param>
    public Action_RushPathComplete(aa_Rush owner, bool getNodeOnComplete)
    {
        this.owner = owner;
        this.getNodeOnComplete = getNodeOnComplete;
    }

    public override Status Check(float dt)
    {
        owner.PathComplete(getNodeOnComplete);

        status = Status.SUCCESS;
        return status;
    }
}
