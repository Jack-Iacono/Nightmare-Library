using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputerController : MonoBehaviour, IClickable
{
    private bool inUse = false;
    [SerializeField]
    private Transform cameraPosition;
    [SerializeField]
    private Canvas computerUI;
    [SerializeField]
    private GameObject screenGameObject;

    public event IClickable.OnClickDelegate OnClick;

    private List<ComputerWindow> openWindows = new List<ComputerWindow>();

    public delegate void ComputerStateDelegate(bool b);
    public static event ComputerStateDelegate OnComputerStateChange;

    private void Awake()
    {
        IClickable.instances.Add(screenGameObject, this);

        computerUI.gameObject.SetActive(false);

        ComputerWindow.OnWindowOpen += OnWindowOpen;
        ComputerWindow.OnWindowClose += OnWindowClose;

        CameraController.OnCameraMoveFinish += OnCameraMoveFinish;
    }

    private void Update()
    {
        if (inUse && PlayerController.mainPlayerInstance.CheckMoveInput())
        {
            SetUseState(false);
        }
    }

    public void SetUseState(bool b)
    {
        if (b)
        {
            PlayerController.mainPlayerInstance.Lock(true, cameraPosition);
        }
        else
        {
            computerUI.gameObject.SetActive(false);
            PlayerController.mainPlayerInstance.Lock(false);

            for(int i = openWindows.Count - 1; i >= 0; i--)
            {
                openWindows[i].Close();
            }
        }

        GameUIController.ActivateHUD(!b);
        inUse = b;
        Cursor.lockState = b ? CursorLockMode.Confined : CursorLockMode.Locked;

        OnComputerStateChange?.Invoke(b);
    }

    public void Click()
    {
        if ((!inUse))
        {
            SetUseState(true);
            OnClick?.Invoke(this);
        }
    }
    public void Hover(bool enterExit){ }

    private void OnWindowClose(ComputerWindow window)
    {
        if(openWindows.Contains(window))
            openWindows.Remove(window);
    }
    private void OnWindowOpen(ComputerWindow window)
    {
        if (!openWindows.Contains(window))
            openWindows.Add(window);
    }

    private void OnCameraMoveFinish(CameraController cam)
    {
        if (inUse)
        {
            computerUI.gameObject.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        IClickable.instances.Remove(screenGameObject);

        if (inUse && PlayerController.mainPlayerInstance != null)
        {
            Cursor.lockState = CursorLockMode.Locked;
            PlayerController.mainPlayerInstance.Lock(false);
        }
    }
}
