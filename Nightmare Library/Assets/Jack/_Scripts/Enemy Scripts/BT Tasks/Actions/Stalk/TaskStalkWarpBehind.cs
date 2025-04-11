 using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TaskStalkWarpBehind : Node
{
    private aa_Stalk owner;
    private NavMeshAgent navAgent;
    private Transform transform;
    private Enemy enemy;

    private float speed;
    private float acceleration;

    private bool hasWarped = false;

    private AudioClip warpSound;

    public TaskStalkWarpBehind(aa_Stalk owner, Enemy enemy, float speed = 20, float acceleration = 400)
    {
        this.owner = owner;
        this.enemy = enemy;
        navAgent = enemy.navAgent;
        transform = navAgent.transform;
        this.speed = speed;
        this.acceleration = acceleration;
    }
    public override Status Check(float dt)
    {
        if (!hasWarped)
        {
            navAgent.Warp(EnemyNavGraph.GetOutOfSightNode(owner.currentTargetPlayer).position);
            navAgent.speed = 0;
            hasWarped = true;

            // Play the sound to alert the player
            AudioManager.PlaySoundAtPoint(AudioManager.GetAudioData(AudioManager.SoundType.e_STALK_APPEAR), owner.currentTargetPlayer.transform.position);
        }

        status = Status.SUCCESS;
        return status;
    }

    protected override void OnResetNode()
    {
        hasWarped = false;
        base.OnResetNode();
    }
}
