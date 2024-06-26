using UnityEngine;
using BehaviorTree;

public class TaskWait : Node
{
    private float waitTime;
    private float waitTimer;

    private string waitLabel;

    public TaskWait(string waitLabel, float waitTime)
    {
        this.waitLabel = waitLabel;
        this.waitTime = waitTime;
    }

    public override Status Check(float dt)
    {
        if (GetData(waitLabel) == null)
            parent.SetData(waitLabel, false);

        if ((bool)GetData(waitLabel))
        {
            waitTimer -= dt;
            if (waitTimer > 0)
            {
                OnTick(waitTimer);

                status = Status.RUNNING;
                return status;
            }

            OnEnd();
            parent.SetData(waitLabel, false);

            status = Status.SUCCESS;
            return status;
        }
        else
        {
            waitTimer = waitTime;
            parent.SetData(waitLabel, true);

            OnStart();

            status = Status.RUNNING;
            return status;
        }
    }

    protected virtual void OnStart() { }
    protected virtual void OnEnd() { }
    protected virtual void OnTick(float time) { }

}
