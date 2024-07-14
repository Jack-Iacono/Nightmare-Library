using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class IdolController : Interactable
{
    [NonSerialized]
    public TaskSpawnIdols idolSpawner;

    public event EventHandler OnInitialized;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void Initialize(TaskSpawnIdols idolSpawner)
    {
        this.idolSpawner = idolSpawner;
        OnInitialized?.Invoke(this, EventArgs.Empty);
    }

    public override void Click()
    {
        if(NetworkManager.Singleton == null || !NetworkManager.Singleton.IsConnectedClient || NetworkManager.Singleton.IsServer)
            RemoveIdol();

        base.Click();
    }

    public void AddIdol()
    {
        gameObject.SetActive(true);
    }
    public void RemoveIdol()
    {
        gameObject.SetActive(false);
        TaskSpawnIdols.RemoveIdol();
    }
}
