using BehaviorTree;
using System.Diagnostics;

public class TaskRushGetNewPath : Node
{
    private aa_Rush owner;

    public TaskRushGetNewPath(aa_Rush owner)
    {
        this.owner = owner;
    }

    public override Status Check(float dt)
    {
        owner.RefreshPath();

        status = Status.SUCCESS;
        return status;
    }
}
