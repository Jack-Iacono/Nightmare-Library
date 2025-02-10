using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreGameUIController : UIController
{
    [Header("Monitor Variabels")]
    public GameObject viewBlocker;

    public float visibleRadius = 7.5f;
    private float playerCheckTime = 0.25f;
    private float playerCheckTimer = 0.25f;
    private LayerMask playerMask = 1 << 6;

    protected override void Awake()
    {
        // Remove the instance functionality
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        playerCheckTimer = playerCheckTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerCheckTimer > 0)
            playerCheckTimer -= Time.deltaTime;
        else
        {
            CheckPlayerInRange();
            playerCheckTimer = playerCheckTime;
        }
    }

    private void OnCameraBroadcastChange(bool broadcasting)
    {
        CheckPlayerInRange();
    }

    private void CheckPlayerInRange()
    {
        bool inRange = Physics.OverlapSphere(transform.position, visibleRadius, playerMask).Length > 0;
        viewBlocker.SetActive(!inRange);
    }
}
