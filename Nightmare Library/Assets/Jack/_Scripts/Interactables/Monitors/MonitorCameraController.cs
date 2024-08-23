using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonitorCameraController : Interactable
{
    [Header("Monitor Variables")]
    public Camera cam;
    public GameObject body;
    [SerializeField]
    private float placementRange = 7f;
    public RenderTexture renderTexture {  get; private set; }

    private bool isBroadcasting = true;
    private static MonitorCameraController pickedUpMonitor;

    private float placeBufferTime = 0.5f;
    private float placeBufferTimer = 0.5f;
    private bool placeBuffering = false;

    public LayerMask placeableLayers;

    private void Awake()
    {
        renderTexture = new RenderTexture(240, 240, 1);
        cam.targetTexture = renderTexture;
    }

    public override void Click()
    {
        base.Click();

        pickedUpMonitor = this;
        Enable(false);
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
                Debug.Log("Placing");
                Ray ray = PlayerController.ownerInstance.camCont.GetCameraRay();
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, placementRange, placeableLayers))
                {
                    transform.position = hit.point;
                    transform.LookAt(PlayerController.ownerInstance.transform.position);

                    pickedUpMonitor = null;
                    Enable(true);
                }
            }
        }
    }

    public void Enable(bool b)
    {
        body.SetActive(b);

        if(!b)
        {
            placeBufferTimer = placeBufferTime;
            placeBuffering = true;
        }
    }
    public void SetBroadcasting(bool b)
    {
        isBroadcasting = b;
        cam.enabled = b;
    }

    private void OnDestroy()
    {
        pickedUpMonitor = null;
    }
}
