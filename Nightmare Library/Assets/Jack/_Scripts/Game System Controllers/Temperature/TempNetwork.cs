using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(TempController))]
public class TempNetwork : NetworkBehaviour
{
    private TempController tempCont;

    private void Awake()
    {
        if (!NetworkConnectionController.IsRunning)
        {
            Destroy(this);
            Destroy(GetComponent<NetworkObject>());
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        tempCont = GetComponent<TempController>();

        if (IsOwner)
            TempController.OnTempChanged += OnTempChanged;
        else
            tempCont.enabled = false;

        TempController.OnTempStateChanged += OnTempStateChanged;
    }

    private void OnTempChanged(int temp)
    {
        OnTempChangedClientRpc(temp);
    }
    [ClientRpc]
    private void OnTempChangedClientRpc(int temp)
    {
        if (!IsOwner)
            TempController.SetTemp(temp);
    }

    private void OnTempStateChanged(int state, bool fromServer)
    {
        if (!fromServer)
        {
            if (IsOwner)
                OnTempStateChangedClientRpc(state);
            else
                OnTempStateChangedServerRpc(state);
        }
    }
    [ClientRpc]
    private void OnTempStateChangedClientRpc(int state)
    {
        if(!IsOwner)
            TempController.SetState(state);
    }
    [ServerRpc(RequireOwnership = false)]
    private void OnTempStateChangedServerRpc(int state)
    {
        TempController.SetState(state);
        OnTempStateChangedClientRpc(state);
    }

    public override void OnDestroy()
    {
        TempController.OnTempChanged -= OnTempChanged;
        TempController.OnTempStateChanged -= OnTempStateChanged;

        base.OnDestroy();
    }
}
