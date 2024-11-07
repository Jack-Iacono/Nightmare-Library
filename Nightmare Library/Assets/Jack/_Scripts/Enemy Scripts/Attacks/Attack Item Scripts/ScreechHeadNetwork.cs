using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(ScreechHeadController))]
public class ScreechHeadNetwork : NetworkBehaviour
{
    ScreechHeadController parent;

    private void Awake()
    {
        parent = GetComponent<ScreechHeadController>();
        parent.OnSpawnHead += OnSpawnHead;
        parent.OnDespawnHead += OnDespawnHead;
        parent.OnAttack += OnAttack;
    }

    public void OnAttack(bool fromNetwork = false)
    {
        
    }
    

    public void OnSpawnHead(Vector3 offset, bool fromNetwork = false)
    {
        
    }
    public void OnDespawnHead(bool fromNetwork = false)
    {
        
    }
}
