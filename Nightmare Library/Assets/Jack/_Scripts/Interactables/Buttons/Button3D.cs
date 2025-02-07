using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Button3D : Interactable
{
    [Space(10)]
    [Header("Button Variables")]
    [SerializeField]
    private TMP_Text text;
    
    [Space(10)]
    [SerializeField]
    private ButtonClickedEvent m_OnClick = new ButtonClickedEvent();

    private Image image;


    protected override void Awake()
    {
        base.Awake();
        image = GetComponent<Image>();
    }

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
    public override void Hover(bool onOff)
    {
        base.Hover(onOff);
    }

    public void SetText(string text)
    {
        this.text.text = text;
    }
    public void SetDelegate(ButtonClickedEvent e)
    {
        m_OnClick = e;
    }
    public void SetColor(Color color)
    {
        image.color = color;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        throw new NotImplementedException();
    }
}
