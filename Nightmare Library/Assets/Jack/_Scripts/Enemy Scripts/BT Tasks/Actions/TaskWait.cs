using UnityEngine;
using BehaviorTree;

public class TaskWait : Node
{
    protected float waitTime { get; private set; }
    protected float waitTimer { get; private set; }
    protected float waitDiff { get; private set; }

    protected string waitLabel { get; private set; }

    public TaskWait(string waitLabel, float waitTime, float waitDiff = 0)
    {
        this.waitLabel = waitLabel;
        this.waitTime = waitTime;
        this.waitDiff = waitDiff;
    }

    public override Status Check(float dt)
    {
        if (TickCondition())
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
                // Gets a random time within the given contraints
                waitTimer = Random.Range(waitTime + waitDiff / 2, waitTime - waitDiff / 2);
                parent.SetData(waitLabel, true);

                OnStart();

                status = Status.RUNNING;
                return status;
            }
        }

        parent.SetData(waitLabel, false);
        waitTimer = 0;

        status = Status.FAILURE;
        return status;
    }

    protected virtual void OnStart() { }
    protected virtual void OnEnd() { }
    protected virtual void OnTick(float time) { }

    protected virtual bool TickCondition() { return true; }

}
