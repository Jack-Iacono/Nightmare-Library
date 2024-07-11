using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(IdolController))]
public class IdolNetwork : NetworkBehaviour
{
    private IdolController parent;

    private void Awake()
    {
        parent = GetComponent<IdolController>();

        parent.OnHit += OnHit;
        parent.OnClick += OnClick;

        parent.OnInitialized += OnParentInitialized;
    }

    private void OnParentInitialized(object sender, EventArgs e)
    {
        if (IsOwner)
            parent.idolSpawner.OnIdolCountChanged += OnIdolCountChanged;
    }

    private void OnClick(object sender, EventArgs e)
    {
        if (!IsOwner)
            TransmitClickServerRpc(NetworkManager.LocalClientId);
    }
    private void OnHit(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    private void OnIdolCountChanged(object sender, int e)
    {
        if (IsOwner)
        {
            ConsumeIdolCountChangeClientRpc(gameObject.activeInHierarchy);
        }
    }
    [ClientRpc]
    private void ConsumeIdolCountChangeClientRpc(bool activeState)
    {
        gameObject.SetActive(activeState);
    }


    [ServerRpc(RequireOwnership = false)]
    private void TransmitClickServerRpc(ulong sender)
    {
        parent.RemoveIdol();
        //ConsumeClickClientRpc(sender);
    }
}
