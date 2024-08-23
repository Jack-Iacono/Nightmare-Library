using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonitorController : MonoBehaviour
{
    public GameObject monitorViewBlocker;
    public RawImage monitorPicture;

    public List<MonitorCameraController> pairedBooks = new List<MonitorCameraController>();
    private int pairedIndex = 0;

    public float useRadius = 5f;
    private float playerCheckTime = 0.25f;
    private float playerCheckTimer = 1;
    private LayerMask playerMask = 1 << 6;

    private RenderTexture display;

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < pairedBooks.Count; i++)
        {
            pairedBooks[i].SetBroadcasting(false);
        }

        ChangeCamera(0);
        playerCheckTimer = playerCheckTime;
    }

    // Update is called once per frame
    void Update()
    {
        if(playerCheckTimer > 0)
            playerCheckTimer -= Time.deltaTime;
        else
        {
            CheckPlayerInRange();
            playerCheckTimer = playerCheckTime;
        }
    }

    private void CheckPlayerInRange()
    {
        bool inRange = Physics.OverlapSphere(transform.position, useRadius, playerMask).Length > 0;
        pairedBooks[pairedIndex].SetBroadcasting(inRange);
        monitorViewBlocker.SetActive(!inRange);
    }
    public void ChangeCamera(int i)
    {
        pairedIndex = i;
        display = pairedBooks[pairedIndex].renderTexture;
        monitorPicture.texture = display;
    }
}
