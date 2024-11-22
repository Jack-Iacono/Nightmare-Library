using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(IdolController))]
public class IdolNetwork : InteractableNetwork
{
    private IdolController idolCont;

    public override void OnNetworkSpawn()
    {
        
        if (!NetworkConnectionController.connectedToLobby)
        {
            Destroy(this);
            Destroy(GetComponent<NetworkObject>());
        }
        else
        {
            idolCont = GetComponent<IdolController>();

            if (IsOwner)
                idolCont.OnIdolActivated += OnIdolActivated;

            PrefabHandlerNetwork.AddSpawnedPrefab(GetComponent<NetworkObject>());
        }

        base.OnNetworkSpawn();
    }

    protected override void OnClick(bool fromNetwork = false)
    {
        if (!IsOwner)
            TransmitClickServerRpc(NetworkManager.LocalClientId);
    }
    [ServerRpc(RequireOwnership = false)]
    protected override void TransmitClickServerRpc(ulong sender)
    {
        idolCont.Remove();
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
