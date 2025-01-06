using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Button3D : Interactable
{
    [Space(10)]
    [SerializeField]
    private ButtonClickedEvent m_OnClick = new ButtonClickedEvent();

    [Serializable]
    public class ButtonClickedEvent : UnityEvent { }
    public ButtonClickedEvent onClick
    {
        get { return m_OnClick; }
        set { m_OnClick = value; }
    }

    public override void Click(bool fromNetwork = false)
    {
        base.Click(fromNetwork);

        m_OnClick?.Invoke();
    }
}
