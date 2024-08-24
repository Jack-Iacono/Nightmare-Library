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
    public GameObject body;
    [SerializeField]
    private float placementRange = 7f;
    public RenderTexture renderTexture {  get; private set; }

    public bool isBroadcasting { get; private set; } = true;
    private static MonitorCameraController pickedUpMonitor;

    private float placeBufferTime = 0.5f;
    private float placeBufferTimer = 0.5f;
    private bool placeBuffering = false;

    public LayerMask placeableLayers;

    public delegate void OnPickupDelegate();
    public event OnPickupDelegate OnPickup;
    public delegate void OnPlaceDelegate(Vector3 pos, Quaternion rot);
    public event OnPlaceDelegate OnPlace;

    public event EventHandler OnBroadcastChange;

    private void Awake()
    {
        renderTexture = new RenderTexture(240, 240, 1);
        cam.targetTexture = renderTexture;
    }

    public override void Click()
    {
        base.Click();

        if (pickedUpMonitor == null)
        {
            pickedUpMonitor = this;
            Pickup();
        }
    }
    protected override void Update()
    {
        base.Update();

        if (placeBuffering)
        {
            placeBufferTimer -= Time.deltaTime;
            if (placeBufferTimer < 0)
                placeBuffering = false;
        }
        else
        {
            if (pickedUpMonitor == this && Input.GetKeyDown(PlayerController.keyInteract))
            {
                Ray ray = PlayerController.ownerInstance.camCont.GetCameraRay();
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, placementRange, placeableLayers))
                {
                    transform.position = hit.point;
                    transform.LookAt(PlayerController.ownerInstance.transform.position);

                    pickedUpMonitor = null;
                    Place(hit.point, transform.rotation);
                }
            }
        }
    }

    public void Pickup(bool sendEvent = true)
    {
        body.SetActive(false);
        placeBufferTimer = placeBufferTime;
        placeBuffering = true;

        SetBroadcasting(false);

        if (sendEvent)
            OnPickup?.Invoke();
    }
    public void Place(Vector3 pos, Quaternion rot, bool sendEvent = true)
    {
        transform.position = pos;
        transform.rotation = rot;

        body.SetActive(true);

        SetBroadcasting(true);

        if (sendEvent)
            OnPlace?.Invoke(pos, rot);
    }

    public void SetBroadcasting(bool b)
    {
        isBroadcasting = b;
        cam.enabled = b;

        OnBroadcastChange?.Invoke(this, EventArgs.Empty);
    }

    private void OnDestroy()
    {
        pickedUpMonitor = null;
    }
}
