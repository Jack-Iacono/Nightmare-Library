using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class ScreechHeadController : MonoBehaviour
{
    private pa_Screech parent;
    private AudioSourceController audioController;

    private Vector3 offset = Vector3.zero;
    public PlayerController targetPlayer;
    private Transform playerCamTrans;
    private Transform trans;

    public bool isSpawned = false;
    private float sightTolerance = -0.94f;

    // Stops this item from attacking if it doesn't need to
    private bool doAttacks = true;

    private MeshRenderer[] meshRenderers;

    private float attackTimer = 0;

    // Used to stop overflow
    public bool hasAttacked = false;

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

        DespawnHead();
        OnInitialize?.Invoke();
    }
    public void Initialize(PlayerController t)
    {
        Debug.Log("Syncing to player: " + t.name);
        
        

        targetPlayer = t;
        playerCamTrans = targetPlayer.camCont.transform;
        trans = transform;

        DespawnHead();
    }

    // Update is called once per frame
    void Update()
    {
        if (targetPlayer == null)
            DespawnHead();

        if (isSpawned)
        {
            trans.position = targetPlayer.transform.position + offset + Vector3.up;

            // Is the player looking at this object
            if(Vector3.Dot(trans.forward, playerCamTrans.forward) < sightTolerance)
            {
                DespawnHead();
            }

            if (doAttacks)
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
        this.offset = offset;
        isSpawned = true;
        transform.position = targetPlayer.transform.position + offset + Vector3.up;
        transform.LookAt(targetPlayer.transform.position + Vector3.up);

        audioController.PlaySound(AudioManager.GetAudioData(AudioManager.SoundType.TEST_SOUNDS), trans.position);

        EnableMesh(true);

        OnSpawnHead?.Invoke(offset);
    }
    public void DespawnHead()
    {
        isSpawned = false;
        EnableMesh(false);

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
