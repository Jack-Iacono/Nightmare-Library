using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(IdolController))]
public class IdolNetwork : NetworkBehaviour
{
    private IdolController parent;

    private void Start()
    {
        parent = GetComponent<IdolController>();

        parent.OnHit += OnHit;
        parent.OnClick += OnClick;

        if (IsOwner)
            parent.OnIdolActivated += OnIdolActivated;

        Debug.Log(IsOwner);
    }

    private void OnHit(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }
    private void OnClick(object sender, EventArgs e)
    {
        if (!IsOwner)
            TransmitClickServerRpc(NetworkManager.LocalClientId);
    }
    [ServerRpc(RequireOwnership = false)]
    private void TransmitClickServerRpc(ulong sender)
    {
        parent.RemoveIdol();
    }

    private void OnIdolActivated(object sender, bool b)
    {
        if (IsOwner)
        {
            ConsumeIdolCountChangeClientRpc(b);
        }
    }
    [ClientRpc]
    private void ConsumeIdolCountChangeClientRpc(bool activeState)
    {
        gameObject.SetActive(activeState);
    }


    

    public override void OnDestroy()
    {
        base.OnDestroy();
    }
}
