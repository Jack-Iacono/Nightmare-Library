using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreechHeadController : MonoBehaviour
{
    private pa_Screech parent;

    private Vector3 offset = Vector3.zero;
    private PlayerController playerController;
    private Transform playerCamTrans;
    private Transform trans;

    private bool isSpawned = false;
    private float sightTolerance = -0.94f;

    private MeshRenderer[] meshRenderers;

    private float currentChance = 0;
    private float cooldownTimer = 0;

    private void Awake()
    {
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
    }
    public void Initialize(pa_Screech parent, PlayerController player)
    {
        this.parent = parent;
        playerController = player;
        playerCamTrans = playerController.camCont.transform;
        trans = transform;

        currentChance = parent.baseChance;
        cooldownTimer = 0;

        DespawnHead();
    }

    // Update is called once per frame
    void Update()
    {
        if (isSpawned)
        {
            trans.position = playerController.transform.position + offset + Vector3.up;

            // Is the player looking at this object
            if(Vector3.Dot(trans.forward, playerCamTrans.forward) < sightTolerance)
            {
                DespawnHead();
            }
        }
        else
        {
            
        }
    }

    private void CheckSpawn()
    {
        
    }

    public void SpawnHead(Vector3 offset)
    {
        this.offset = offset;
        isSpawned = true;
        transform.position = playerController.transform.position + offset + Vector3.up;
        transform.LookAt(playerController.transform.position + Vector3.up);

        EnableMesh(true);
    }
    public void DespawnHead()
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
