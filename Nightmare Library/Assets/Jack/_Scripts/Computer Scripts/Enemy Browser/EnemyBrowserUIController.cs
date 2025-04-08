using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ComputerWindow))]
public class EnemyBrowserUIController : UIController
{
    private ComputerWindow window;

    [SerializeField]
    private Button forwardButton;
    [SerializeField]
    private Button backButton;

    private LinkedList<int> links = new LinkedList<int>();

    protected override void Awake()
    {
        window = GetComponent<ComputerWindow>();
        ComputerWindow.OnWindowOpen += OnWindowOpen;
    }

    private void OnWindowOpen(ComputerWindow window)
    {
        links.Clear();
        ChangeToScreen(0);
    }

    private void CheckPageButtons()
    {
        forwardButton.gameObject.SetActive(links.Find(currentScreen) != links.Last);
        backButton.gameObject.SetActive(links.Find(currentScreen) != links.First);
    }

    public override void ChangeToScreen(int i)
    {
        if (links.Count == 0)
            links.AddFirst(i);
        else if(links.Last.Value == currentScreen)
            links.AddLast(i);
        else if(links.Find(currentScreen).Next.Value != i)
            links.Find(currentScreen).Next.Value = i;

        base.ChangeToScreen(i);
        CheckPageButtons();
    }

    public void PageBack()
    {
        base.ChangeToScreen(links.Find(currentScreen).Previous.Value);
        CheckPageButtons();
    }
    public void PageForward()
    {
        base.ChangeToScreen(links.Find(currentScreen).Next.Value);
        CheckPageButtons();
    }
}
