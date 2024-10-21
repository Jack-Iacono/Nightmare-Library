using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;
using UnityEngine.AI;

public class CheckTargetInRange : Node
{
    protected Transform transform;
    protected ActiveAttack owner;

    protected float range = 10;

    protected bool passCheck = false;

    public CheckTargetInRange(ActiveAttack owner, Transform transform, float range = 10)
    {
        this.owner = owner;
        this.range = range;
        this.transform = transform;
    }
    public override Status Check(float dt)
    {
        if (passCheck || (owner.currentTargetDynamic != null && Vector3.SqrMagnitude(transform.position - owner.currentTargetDynamic.position) < range * range))
        {
            if (!passCheck)
                InRangeAction();

            passCheck = true;

            status = Status.SUCCESS;
            return status;
        }

        status = Status.FAILURE;
        return status;
    }

    protected virtual void InRangeAction() { }

    protected override void OnResetNode()
    {
        base.OnResetNode();
        passCheck = false;
    }
}
