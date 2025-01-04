using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class ScreechHeadController : MonoBehaviour
{
    public pa_Screech parent { get; private set; }
    private AudioSourceController audioController;

    private Vector3 offset = Vector3.zero;
    public PlayerController targetPlayer;
    private Transform playerCamTrans;
    private Transform trans;

    public bool isSpawned = false;
    private float sightTolerance = -0.94f;

    private MeshRenderer[] meshRenderers;

    private float attackTimer = 0;

    // Used to stop overflow
    public bool hasAttacked = false;

    private bool doAttack = false;
    private bool doSight = false;

    public delegate void SpawnHeadDelegate(Vector3 pos);
    public event SpawnHeadDelegate OnSpawnHead;

    public delegate void DespawnHeadDelegate();
    public event DespawnHeadDelegate OnDespawnHead;

    public delegate void AttackDelegate();
    public event AttackDelegate OnAttack;

    public delegate void InitializeDelegate();
    public event InitializeDelegate OnInitialize;

    private void Awake()
    {
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        audioController = GetComponent<AudioSourceController>();
    }
    public void Initialize(pa_Screech parent, PlayerController player)
    {
        this.parent = parent;
        targetPlayer = player;
        playerCamTrans = targetPlayer.camCont.transform;
        trans = transform;

        attackTimer = parent.attackTime;
        doAttack = true;
        doSight = true;

        DespawnHead(true);
        OnInitialize?.Invoke();
    }
    public void Initialize(PlayerController player, bool owner, bool server)
    {
        targetPlayer = player;
        playerCamTrans = targetPlayer.camCont.transform;
        trans = transform;

        doAttack = server;
        doSight = owner;

        DespawnHead(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (targetPlayer == null || !targetPlayer.isAlive)
            DespawnHead();

        if (isSpawned)
        {
            trans.position = targetPlayer.transform.position + offset + Vector3.up;

            // Is the player looking at this object
            if(doSight && Vector3.Dot(trans.forward, playerCamTrans.forward) < sightTolerance)
            {
                DespawnHead();
            }

            if (doAttack)
            {
                if (attackTimer <= 0)
                {
                    if (!hasAttacked)
                    {
                        Attack();
                    }
                    attackTimer = parent.attackTime;
                }
                else
                    attackTimer -= Time.deltaTime;
            }
        }
    }

    public void Attack()
    {
        if(parent != null)
            parent.AttackPlayer(targetPlayer);
        hasAttacked = true;

        OnAttack?.Invoke();
    }

    public void SpawnHead(Vector3 offset)
    {
        Debug.Log("Spawning Head");

        this.offset = offset;
        isSpawned = true;
        transform.position = targetPlayer.transform.position + offset + Vector3.up;
        transform.LookAt(targetPlayer.transform.position + Vector3.up);

        audioController.PlaySound(AudioManager.GetAudioData(AudioManager.SoundType.TEST_SOUNDS), trans.position);

        EnableMesh(true);

        OnSpawnHead?.Invoke(offset);
    }
    public void DespawnHead(bool fromNetwork = false)
    {
        isSpawned = false;
        EnableMesh(false);

        if(doAttack)
            attackTimer = parent.attackTime;

        if (!fromNetwork)
            OnDespawnHead?.Invoke();
    }

    public void EnableMesh(bool b)
    {
        foreach(MeshRenderer renderer in meshRenderers)
        {
            renderer.enabled = b;
        }
    }
}
