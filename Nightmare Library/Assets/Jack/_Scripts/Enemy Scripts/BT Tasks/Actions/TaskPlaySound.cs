using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;
using static AudioManager;
using static UnityEngine.UI.GridLayoutGroup;

public class TaskPlaySound : Node
{
    protected SoundType soundType;

    // Used to differentiate between using the dynamically called delegate and the listed transform
    protected bool useTransform = false;

    protected Transform trans = null;

    public delegate Vector3 GetPositionDelegate();
    protected GetPositionDelegate GetPosition;

    private bool hasPlayed = false;

    public TaskPlaySound(SoundType sound, Transform dynamicTarget)
    {
        soundType = sound;
        trans = dynamicTarget;

        useTransform = true;
    }
    public TaskPlaySound(SoundType sound, GetPositionDelegate targetPositionDelegate)
    {
        soundType = sound;
        GetPosition = targetPositionDelegate;

        useTransform = false;
    }

    public override Status Check(float dt)
    {
        if(!hasPlayed)
        {
            if (useTransform)
            {
                PlaySoundAtPoint(GetAudioData(SoundType.e_STALK_APPEAR), trans.position);
            }
            else
            {
                PlaySoundAtPoint(GetAudioData(SoundType.e_STALK_APPEAR), GetPosition());
            }

            hasPlayed = true;
        }

        return Status.SUCCESS;
    }

    protected override void OnResetNode()
    {
        base.OnResetNode();
        hasPlayed = false;
    }
}
