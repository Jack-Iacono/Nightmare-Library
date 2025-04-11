using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ComputerWindow : EventTrigger
{
    public enum WindowState { Open, Closed }
    public WindowState state { get; private set; }

    private bool cursorInWindow = false;

    public delegate void WindowEventDelegate(ComputerWindow window);
    public static event WindowEventDelegate OnWindowOpen;
    public static event WindowEventDelegate OnWindowClose;

    private void Start()
    {
        Close();
    }

    public override void OnPointerEnter(PointerEventData data)
    {
        base.OnPointerEnter(data);
    }
    public override void OnPointerExit(PointerEventData data)
    {
        base.OnPointerExit(data);
    }
    public override void OnPointerClick(PointerEventData eventData)
    {
        SetActiveWindow();
        if (cursorInWindow)
        {
            base.OnPointerClick(eventData);
        }
    }

    public void Open()
    {
        state = WindowState.Open;
        enabled = true;
        gameObject.SetActive(true);

        SetActiveWindow();

        OnWindowOpen?.Invoke(this);
    }
    public void Close()
    {
        state = WindowState.Closed;
        enabled = false;
        gameObject.SetActive(false);

        OnWindowClose?.Invoke(this);
    }

    public void SetActiveWindow()
    {
        transform.SetAsLastSibling();
    }
}
