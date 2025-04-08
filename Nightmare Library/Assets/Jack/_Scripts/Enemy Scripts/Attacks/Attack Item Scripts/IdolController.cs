using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class IdolController : MonoBehaviour, IClickable
{
    public static List<IdolController> allIdols = new List<IdolController>();

    public bool isActive = false;
    public event IClickable.OnClickDelegate OnClick;

    public delegate void OnActiveStateChangedDelegate();
    public event OnActiveStateChangedDelegate OnActiveStateChanged;

    protected void Awake()
    {
        allIdols.Add(this);
        IClickable.instances.Add(gameObject, this);
    }

    private void Start()
    {
        gameObject.SetActive(false);
        isActive = false;
    }

    public void Click()
    {
        if (NetworkConnectionController.HasAuthority)
            Remove();
        else
            Activate(false);
        OnClick?.Invoke(this);
    }
    public void Hover(bool enterExit) { }

    public void Activate(bool b)
    {
        gameObject.SetActive(b);
        isActive = b;

        OnActiveStateChanged?.Invoke();
    }
    public void Remove()
    {
        Activate(false);
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
        IClickable.instances.Remove(gameObject);
    }
}
