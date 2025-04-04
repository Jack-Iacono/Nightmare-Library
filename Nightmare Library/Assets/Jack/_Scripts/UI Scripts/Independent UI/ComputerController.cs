using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputerController : MonoBehaviour, IClickable
{
    private bool inUse = false;
    [SerializeField]
    private Transform cameraPosition;
    [SerializeField]
    private ComputerUIController uiController;

    public event IClickable.OnClickDelegate OnClick;

    private void Awake()
    {
        IClickable.instances.Add(gameObject, this);
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
            //uiController.ChangeToScreen(0);
        }
        else
        {
            PlayerController.mainPlayerInstance.Lock(false);
            //uiController.ChangeToScreen(0);
        }

        GameUIController.ActivateHUD(!b);
        inUse = b;
        Cursor.lockState = b ? CursorLockMode.Confined : CursorLockMode.Locked;
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

    private void OnDestroy()
    {
        IClickable.instances.Remove(gameObject);
    }
}
