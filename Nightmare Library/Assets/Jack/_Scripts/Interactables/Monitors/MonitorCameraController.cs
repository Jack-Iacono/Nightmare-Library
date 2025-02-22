using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MonitorCameraController : Interactable
{
    [Header("Monitor Variables")]
    public Camera cam;
    public RenderTexture renderTexture {  get; private set; }
    public bool isBroadcasting { get; private set; } = true;
    public delegate void OnBroadcastChangeDelegate(bool broadcast);
    public event OnBroadcastChangeDelegate OnBroadcastChange;

    protected override void Awake()
    {
        base.Awake();

        renderTexture = new RenderTexture(240, 240, 1);
        cam.targetTexture = renderTexture;
    }

    public override GameObject Pickup(bool fromNetwork = false)
    {
        SetBroadcasting(false);
        return base.Pickup(fromNetwork);
    }
    public override void Place(bool fromNetwork = false)
    {
        SetBroadcasting(true);
        base.Place(fromNetwork);
    }
    public override void Throw(Vector3 pos, Vector3 force, bool fromNetwork = false)
    {
        SetBroadcasting(true);
        base.Throw(pos, force, fromNetwork);
    }

    public void SetBroadcasting(bool b)
    {
        isBroadcasting = b;
        OnBroadcastChange?.Invoke(isBroadcasting);
    }
    public void SetViewing(bool b)
    {
        if (!b)
            cam.enabled = false;
        else if(isBroadcasting)
            cam.enabled = true;
    }
}
