using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LibraryCameraController : HoldableItem
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
    public override void Place(Vector3 pos, Quaternion rot, bool fromNetwork = false)
    {
        SetBroadcasting(true);
        base.Place(pos, rot, fromNetwork);
    }
    public override void Throw(Vector3 pos, Vector3 force, Vector3 rot, bool fromNetwork = false)
    {
        SetBroadcasting(true);
        base.Throw(pos, force, rot, fromNetwork);
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
