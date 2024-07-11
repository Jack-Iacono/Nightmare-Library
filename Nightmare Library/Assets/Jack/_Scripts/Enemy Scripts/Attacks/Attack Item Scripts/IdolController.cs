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

    public void Initialize(TaskSpawnIdols idolSpawner)
    {
        this.idolSpawner = idolSpawner;
        OnInitialized?.Invoke(this, EventArgs.Empty);
    }

    public override void Click()
    {
        if(NetworkManager.Singleton == null || NetworkManager.Singleton.IsServer)
            RemoveIdol();

        base.Click();
    }

    public void RemoveIdol()
    {
        idolSpawner.RemoveIdol();
    }
}
