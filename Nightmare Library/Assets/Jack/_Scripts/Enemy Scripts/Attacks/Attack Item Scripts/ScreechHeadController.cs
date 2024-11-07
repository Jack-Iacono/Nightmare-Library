using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreechHeadController : MonoBehaviour
{
    private pa_Screech parent;

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

    public delegate void SpawnHeadDelegate(Vector3 pos, bool fromNetwork);
    public event SpawnHeadDelegate OnSpawnHead;

    public delegate void DespawnHeadDelegate(bool fromNetwork);
    public event DespawnHeadDelegate OnDespawnHead;

    public delegate void AttackDelegate(bool fromNetwork);
    public event AttackDelegate OnAttack;

    private void Awake()
    {
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
    }
    public void Initialize(pa_Screech parent, PlayerController player)
    {
        this.parent = parent;
        targetPlayer = player;
        playerCamTrans = targetPlayer.camCont.transform;
        trans = transform;

        attackTimer = parent.attackTime;

        DespawnHead();
    }

    // Update is called once per frame
    void Update()
    {
        if (isSpawned)
        {
            trans.position = targetPlayer.transform.position + offset + Vector3.up;

            // Is the player looking at this object
            if(Vector3.Dot(trans.forward, playerCamTrans.forward) < sightTolerance)
            {
                DespawnHead();
            }

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

    public void Attack(bool fromNetwork = false)
    {
        parent.AttackPlayer(targetPlayer);
        hasAttacked = true;
    }

    public void SpawnHead(Vector3 offset, bool fromNetwork = false)
    {
        this.offset = offset;
        isSpawned = true;
        transform.position = targetPlayer.transform.position + offset + Vector3.up;
        transform.LookAt(targetPlayer.transform.position + Vector3.up);

        EnableMesh(true);
    }
    public void DespawnHead(bool fromNetwork = false)
    {
        isSpawned = false;
        EnableMesh(false);
    }

    public void EnableMesh(bool b)
    {
        foreach(MeshRenderer renderer in meshRenderers)
        {
            renderer.enabled = b;
        }
    }
}
