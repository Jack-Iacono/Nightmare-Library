using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyBrowserUIController : UIController
{
    [SerializeField]
    private GameObject forwardButton;
    [SerializeField]
    private GameObject backButton;

    private List<int> screenTrail = new List<int>() { 0 };
    private int trailIndex = 0;

    protected override void Awake()
    {
        ComputerController.OnComputerStateChange += OnComputerStateChanged;
    }

    private void OnComputerStateChanged(bool b)
    {
        if (!b)
        {
            ChangeToScreen(0);

            trailIndex = 0;
            screenTrail = new List<int>() { 0 };
        }
    }

    public override void ChangeToScreen(int i)
    {
        if(i != screenTrail[trailIndex])
        {
            // If this path would contradict a prior path, remove the prior path
            if (screenTrail.Count - 1 > trailIndex)
                screenTrail.RemoveRange(trailIndex, screenTrail.Count - 1);

            if (screenTrail[trailIndex] == i)
                screenTrail.Add(i);
            trailIndex++;
        }

        Debug.Log(trailIndex + " || " + screenTrail.Count);

        forwardButton.SetActive(trailIndex < screenTrail.Count - 1);
        backButton.SetActive(trailIndex > 0);

        base.ChangeToScreen(i);
    }

    public void PageBack()
    {
        if(trailIndex > 0)
            trailIndex--;
        ChangeToScreen(screenTrail[trailIndex]);
    }
    public void PageForward()
    {
        if(screenTrail.Count - 1 > trailIndex)
            trailIndex++;
        ChangeToScreen(screenTrail[trailIndex]);
    }
}
