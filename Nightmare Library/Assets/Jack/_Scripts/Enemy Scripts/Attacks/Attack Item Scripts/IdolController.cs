using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class IdolController : Interactable
{
    public static List<IdolController> allIdols = new List<IdolController>();

    public bool isActive = false;
    public event EventHandler<bool> OnIdolActivated;

    protected override void Awake()
    {
        base.Awake();
        allIdols.Add(this);
    }

    private void Start()
    {
        gameObject.SetActive(false);
        isActive = false;
    }

    public override void Click(bool fromNetwork = false)
    {
        if(NetworkManager.Singleton == null || !NetworkManager.Singleton.IsConnectedClient || NetworkManager.Singleton.IsServer)
            Remove();

        base.Click();
    }

    public void Activate()
    {
        gameObject.SetActive(true);
        isActive = true;
        OnIdolActivated?.Invoke(this, true);
    }
    public void Deactivate()
    {
        gameObject.SetActive(false);
        isActive = false;
        OnIdolActivated?.Invoke(this, false);
    }
    public void Remove()
    {
        gameObject.SetActive(false);
        isActive = false;
        OnIdolActivated?.Invoke(this, false);

        pa_Idols.RemoveIdol();
    }

    public static List<IdolController> GetAllIdols()
    {
        List<IdolController> newList = new List<IdolController>();
        foreach(IdolController i in allIdols)
        {
            newList.Add(i);
        }
        return newList;
    }

    private void OnDestroy()
    {
        allIdols.Remove(this);
    }
}
