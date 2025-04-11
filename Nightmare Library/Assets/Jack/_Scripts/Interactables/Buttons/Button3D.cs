using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Button3D : MonoBehaviour, IClickable
{
    [Space(10)]
    [Header("Button Variables")]
    [SerializeField]
    private TMP_Text text;

    [SerializeField]
    private Collider colliderOverride;
    
    [Space(10)]
    [SerializeField]
    private ButtonClickedEvent m_OnClick = new ButtonClickedEvent();

    private Image image;

    public event IClickable.OnClickDelegate OnClick;

    protected void Awake()
    {
        image = GetComponent<Image>();

        if(colliderOverride != null)
            IClickable.instances.Add(colliderOverride.gameObject, this);
        else
            IClickable.instances.Add(gameObject, this);
    }

    [Serializable]
    public class ButtonClickedEvent : UnityEvent { }
    public ButtonClickedEvent onClick
    {
        get { return m_OnClick; }
        set { m_OnClick = value; }
    }

    public void Click()
    {
        m_OnClick?.Invoke();
        OnClick?.Invoke(this);
    }
    public void Hover(bool onOff)
    {
        
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

    private void OnDestroy()
    {
        IClickable.instances.Remove(gameObject);
    }
}
