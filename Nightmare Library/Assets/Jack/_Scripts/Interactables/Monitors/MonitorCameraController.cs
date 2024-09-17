using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static UnityEditor.PlayerSettings;
using static UnityEngine.Rendering.DebugUI.Table;

public class MonitorCameraController : Interactable
{
    [Header("Monitor Variables")]
    public Camera cam;
    public RenderTexture renderTexture {  get; private set; }
    public bool isBroadcasting { get; private set; } = true;
    public event EventHandler OnBroadcastChange;

    protected override void Awake()
    {
        base.Awake();

        renderTexture = new RenderTexture(240, 240, 1);
        cam.targetTexture = renderTexture;
    }

    public override void Pickup(bool fromNetwork = false)
    {
        SetBroadcasting(false);

        base.Pickup(fromNetwork);
    }
    public override void Place(bool fromNetwork = false)
    {
        SetBroadcasting(true);

        base.Place(fromNetwork);
    }

    public void SetBroadcasting(bool b)
    {
        isBroadcasting = b;
        cam.enabled = b;

        OnBroadcastChange?.Invoke(this, EventArgs.Empty);
    }
}
