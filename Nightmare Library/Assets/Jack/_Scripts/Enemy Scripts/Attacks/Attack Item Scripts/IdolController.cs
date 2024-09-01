using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class IdolController : Interactable
{
    [NonSerialized]
    public TaskSpawnIdols idolSpawner;

    public event EventHandler<bool> OnIdolActivated;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void Initialize(TaskSpawnIdols idolSpawner)
    {
        this.idolSpawner = idolSpawner;
    }

    public override void Click(bool fromNetwork = false)
    {
        if(NetworkManager.Singleton == null || !NetworkManager.Singleton.IsConnectedClient || NetworkManager.Singleton.IsServer)
            RemoveIdol();

        base.Click();
    }

    public void AddIdol()
    {
        gameObject.SetActive(true);
        OnIdolActivated?.Invoke(this, true);
    }
    public void RemoveIdol()
    {
        gameObject.SetActive(false);
        OnIdolActivated?.Invoke(this, false);
        TaskSpawnIdols.RemoveIdol();
    }
}
