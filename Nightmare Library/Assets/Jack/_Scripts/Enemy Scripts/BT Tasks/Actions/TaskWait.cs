using UnityEngine;
using BehaviorTree;

public class TaskWait : Node
{
    protected float waitTimer { get; private set; }
    protected float waitMin { get; private set; }
    protected float waitMax { get; private set; }
    
    /// <summary>
    /// When the timer will restart, 0 = On Reset, 1 = On End
    /// </summary>
    protected int resetType { get; private set; }

    private bool timerFinished = false;

    /// <param name="owner">The Active Attack that owns this timer</param>
    /// <param name="waitTime">The average time that should be waited</param>
    /// <param name="waitDiff">The average deviation from the average time</param>
    /// <param name="timeChange">The amount that this value should change with the Enemy's level</param>
    /// <param name="resetType">When the timer will restart, 0 = On Reset, 1 = On End</param>
    public TaskWait(float waitMin, float waitMax, int resetType = 0)
    {
        this.waitMin = waitMin;
        this.waitMax = waitMax;
        this.resetType = resetType;
    }
    public TaskWait(float wait, int resetType = 0)
    {
        waitMin = wait;
        waitMax = wait;
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
                    waitTimer = Random.Range(waitMin, waitMax);

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

    public void OnLevelChange(float waitMax, float waitMin)
    {
        this.waitMin = waitMin;
        this.waitMax = waitMax;
    }
}
