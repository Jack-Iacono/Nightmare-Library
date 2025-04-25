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
        if (NetworkConnectionController.CheckNetworkConnected(this))
        {
            parent = GetComponent<UIController>();
            parent.OnStartFinish += OnParentStartFinish;
        }
    }

    private void OnParentStartFinish()
    {
        if (!IsOwner)
            parent.ChangeToScreen(screenIndex.Value);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            parent.OnScreenIndexChange += OnScreenChange;
        }
        else
        {
            screenIndex.OnValueChanged += OnScreenIndexChange;
            parent.ChangeToScreen(screenIndex.Value);
        }
    }

    private void OnScreenChange(int index)
    {
        screenIndex.Value = index;
    }
    private void OnScreenIndexChange(int previousValue, int newValue)
    {
        parent.ChangeToScreen(newValue);
    }
}
