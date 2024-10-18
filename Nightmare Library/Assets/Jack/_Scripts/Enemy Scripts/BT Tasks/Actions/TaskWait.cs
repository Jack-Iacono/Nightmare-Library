using UnityEngine;
using BehaviorTree;

public class TaskWait : Node
{
    protected float waitTime { get; private set; }
    protected float waitTimer { get; private set; }
    protected float waitDiff { get; private set; }
    /// <summary>
    /// When the timer will restart, 0 = On Reset, 1 = On End
    /// </summary>
    protected int resetType { get; private set; }

    private bool timerFinished = false;

    public TaskWait(float waitTime, float waitDiff = 0, int resetType = 0)
    {
        this.waitTime = waitTime;
        this.waitDiff = waitDiff;
        this.resetType = resetType;
    }

    public override Status Check(float dt)
    {
        if (TickCondition())
        {
            if (!timerFinished)
            {
                if (waitTimer > 0)
                {
                    waitTimer -= dt;

                    if (waitTimer > 0)
                    {
                        OnTick(waitTimer);

                        status = Status.RUNNING;
                        return status;
                    }

                    if (resetType == 1)
                        ResetTimer();
                    else
                        timerFinished = true;

                    OnEnd();
                }
                else
                {
                    // Gets a random time within the given contraints
                    waitTimer = Random.Range(waitTime + waitDiff / 2, waitTime - waitDiff / 2);

                    OnStart();

                    status = Status.RUNNING;
                    return status;
                }
            }

            status = Status.SUCCESS;
            return status;
        }

        waitTimer = 0;

        status = Status.FAILURE;
        return status;
    }

    private void ResetTimer() 
    {
        timerFinished = false;
        waitTimer = 0;
    }

    protected virtual void OnStart() { }
    protected virtual void OnEnd() { }
    protected virtual void OnTick(float time) { }

    protected virtual bool TickCondition() { return true; }

    protected override void SetParent(Node n)
    {
        base.SetParent(n);
        parent.OnReset += OnResetNode;
    }
    protected override void OnResetNode()
    {
        base.OnResetNode();
        if(resetType == 0)
            ResetTimer();
    }
}
