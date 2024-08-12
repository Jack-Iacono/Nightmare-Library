using System;
using UnityEngine;
using UnityEngine.AI;

using BehaviorTree;
using UnityEditor.Timeline;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Characteristics")]
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

    public enum EvidenceEnum { HYSTERICS, MUSIC_LOVER };
    [Header("Evidence Variables")]
    [SerializeField]
    public List<EvidenceEnum> evidenceList = new List<EvidenceEnum>();

    protected List<Evidence> evidence = new List<Evidence>();

    [Header("Sounds")]
    private AudioSource audioSrc;
    public AudioClip musicLoverSound;

    public event EventHandler<string> OnPlaySound;

    #region Initialization

    private void Start()
    {
        Initialize();
    }
    public virtual void Initialize()
    {
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.speed = moveSpeed;

        audioSrc = GetComponent<AudioSource>();

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

        for(int i = 0; i < evidenceList.Count; i++)
        {
            switch(evidenceList[i])
            {
                case EvidenceEnum.HYSTERICS:
                    evidence.Add(new ev_Hysterics(this));
                    break;
                case EvidenceEnum.MUSIC_LOVER:
                    evidence.Add(new ev_MusicLover(this));
                    break;
            }
        }
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

        foreach (Evidence e in evidence)
        {
            e.UpdateProcess(dt);
        }
    }

    public void Activate(bool b)
    {
        enabled = b;
        if(!navAgent)
            navAgent = GetComponent<NavMeshAgent>();
        navAgent.enabled = b;
    }

    public void PlaySound(string soundName)
    {
        OnPlaySound?.Invoke(this, soundName);

        if(!audioSrc)
            audioSrc = GetComponent<AudioSource>();

        switch (soundName)
        {
            case "musicLover":
                audioSrc.volume = 2;
                audioSrc.PlayOneShot(musicLoverSound);
                break;
        }
    }

    protected virtual void OnGamePause(object sender, bool e)
    {
        if(navAgent.isOnNavMesh)
            navAgent.isStopped = e;
    }

    private void OnDestroy()
    {
        if(activeAttackTree != null)
            activeAttackTree.OnDestroy();
        if(passiveAttackTree != null)
            passiveAttackTree.OnDestroy();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, fovRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
