using BehaviorTree;
using System.Diagnostics;

public class TaskRushGetNextGoal : Node
{
    private aa_Rush owner;
    private bool getNext;

    public TaskRushGetNextGoal(aa_Rush owner, bool getNext = true)
    {
        this.owner = owner;
        this.getNext = getNext;
    }

    public override Status Check(float dt)
    {
        owner.RefreshPath(getNext);

        status = Status.SUCCESS;
        return status;
    }
}
