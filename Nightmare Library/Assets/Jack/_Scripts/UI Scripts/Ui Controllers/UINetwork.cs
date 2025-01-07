using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Android;

[RequireComponent(typeof(UIController))]
public class UINetwork : NetworkBehaviour
{
    private UIController parent;
    private NetworkVariable<int> screenIndex = new NetworkVariable<int>();

    private void Awake()
    {
        if (!NetworkConnectionController.connectedToLobby)
        {
            Destroy(this);
            Destroy(GetComponent<NetworkObject>());
        }

        parent = GetComponent<UIController>();

        if (IsServer)
        {
            parent.OnScreenIndexChange += OnScreenChange;
        }
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner)
            parent.ChangeToScreen(screenIndex.Value);
    }

    private void OnScreenChange(int index)
    {
        Debug.Log("Changing Screen");
        screenIndex.Value = index;
    }
    private void OnCameraIndexChange(int previousValue, int newValue)
    {
        Debug.Log("New Screen Value " + newValue);
        parent.ChangeToScreen(newValue);
    }
}
