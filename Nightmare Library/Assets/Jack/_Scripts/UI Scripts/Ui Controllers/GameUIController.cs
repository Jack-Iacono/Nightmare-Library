using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUIController : UIController
{
    public static GameUIController Instance;

    protected override void Awake()
    {
        if(Instance != null)
            Destroy(Instance);
        Instance = this;

        base.Awake();
    }
    public static void ActivateHUD(bool b)
    {
        if(Instance != null)
        {
            if (b)
                Instance.ChangeToScreen(0);
            else
                Instance.ChangeToScreen(-1);
        }
    }
    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
