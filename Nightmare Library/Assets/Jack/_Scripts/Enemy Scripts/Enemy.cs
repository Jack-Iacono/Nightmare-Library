using System;
using UnityEngine;
using UnityEngine.AI;

using BehaviorTree;
using UnityEditor.Timeline;

public abstract class Enemy : MonoBehaviour
{
    [Header("Nightmare Characteristics")]
    [SerializeField]
    public float moveSpeed = 10;
    public float fovRange = 30;
    public float attackRange = 2;

    [Header("NavMesh Objects")]
    [NonSerialized]
    public NavMeshAgent navAgent;

    protected Vector3 spawnLocation;
    protected Vector3 targetLocation = Vector3.zero;

    protected BehaviorTree.Tree currentTree;

    #region Initialization

    public virtual void Initialize()
    {
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.speed = moveSpeed;

        GameController.OnGamePause += OnGamePause;

        navAgent.Warp(spawnLocation);
    }
    public virtual void Spawn()
    {
        //Spawn Stuff
        navAgent.Warp(spawnLocation);
    }

    #endregion

    public virtual void PlayerDamageSwing() { }
    public virtual void PlayerDamageShoot() { }

    protected virtual void Update()
    {
        currentTree.UpdateTree(Time.deltaTime);
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
