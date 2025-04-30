using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(IdolController))]
public class IdolNetwork : NetworkBehaviour
{
    private IdolController parent;
    private NetworkVariable<bool> isActive = new NetworkVariable<bool>();

    public override void OnNetworkSpawn()
    {
        if (!NetworkConnectionController.connectedToLobby)
        {
            Destroy(this);
            Destroy(GetComponent<NetworkObject>());
        }
        else
        {
            parent = GetComponent<IdolController>();

            if (IsOwner)
            {
                isActive.Value = false;
                parent.OnActiveStateChanged += OnParentActiveStateChanged;
            }
            else
            {
                isActive.OnValueChanged += OnActiveValueChanged;
                parent.OnClick += OnClick;

                if(isActive != null)
                    OnActiveValueChanged(isActive.Value, isActive.Value);
            }

            PrefabHandlerNetwork.AddSpawnedPrefab(GetComponent<NetworkObject>());
        }

        base.OnNetworkSpawn();
    }

    private void OnParentActiveStateChanged()
    {
        isActive.Value = parent.isActive;
    }

    protected void OnClick(IClickable clickable)
    {
        if (!IsOwner)
            TransmitClickServerRpc();
    }
    [ServerRpc(RequireOwnership = false)]
    protected void TransmitClickServerRpc()
    {
        parent.Click();
    }

    private void OnActiveValueChanged(bool previousValue, bool newValue)
    {
        parent.Activate(newValue);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }
}
