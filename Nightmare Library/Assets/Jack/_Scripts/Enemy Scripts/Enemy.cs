using System;
using UnityEngine;
using UnityEngine.AI;

using BehaviorTree;
using UnityEditor.Timeline;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    [Header("Nightmare Characteristics")]
    [SerializeField]
    public float moveSpeed = 10;
    public float fovRange = 100;
    public float attackRange = 2;

    [NonSerialized]
    public NavMeshAgent navAgent;

    [SerializeField]
    protected Vector3 spawnLocation;
    protected Vector3 targetLocation = Vector3.zero;

    public enum aAttackEnum { RUSH };
    public enum pAttackEnum { IDOLS };

    [Header("Attack Variables")]
    [SerializeField]
    public aAttackEnum aAttack;
    [SerializeField]
    public pAttackEnum pAttack;

    protected ActiveAttack activeAttackTree;
    protected PassiveAttack passiveAttackTree;

    #region Initialization

    public virtual void Initialize()
    {
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.speed = moveSpeed;

        GameController.OnGamePause += OnGamePause;

        navAgent.Warp(spawnLocation);

        // Assigning Attacks
        switch (aAttack)
        {
            case aAttackEnum.RUSH:
                activeAttackTree = new aa_Rush(this);
                break;
        }

        switch (pAttack)
        {
            case pAttackEnum.IDOLS:
                passiveAttackTree = new pa_Idols(this);
                break;
        }

        activeAttackTree.Initialize();
        passiveAttackTree.Initialize();
    }
    public virtual void Spawn()
    {
        //Spawn Stuff
        navAgent.Warp(spawnLocation);
    }

    #endregion

    public virtual void PlayerDamageSwing() { }
    public virtual void PlayerDamageShoot() { }

    protected void Update()
    {
        float dt = Time.deltaTime;

        activeAttackTree.UpdateTree(dt);
        passiveAttackTree.UpdateTree(dt);
    }

    public void Activate(bool b)
    {
        enabled = b;
        if(!navAgent)
            navAgent = GetComponent<NavMeshAgent>();
        navAgent.enabled = b;
    }

    protected virtual void OnGamePause(object sender, bool e)
    {
        if(navAgent.isOnNavMesh)
            navAgent.isStopped = e;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, fovRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
