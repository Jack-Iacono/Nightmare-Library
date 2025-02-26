using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class TaskRushRemoveAudioSource : Node
{
    private aa_Rush owner;

    public TaskRushRemoveAudioSource(aa_Rush owner)
    {
        this.owner = owner;
    }

    public override Status Check(float dt)
    {
        owner.RemoveFirstAudioSource();
        return Status.SUCCESS;
    }
}
