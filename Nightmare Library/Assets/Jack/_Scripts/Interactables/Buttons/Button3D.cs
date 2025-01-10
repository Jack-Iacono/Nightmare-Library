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
    [SerializeField]    
    private GameObject gameobjectOverride = null;
    [Space(10)]
    [SerializeField]
    private ButtonClickedEvent m_OnClick = new ButtonClickedEvent();


    protected override void Awake()
    {
        if (gameobjectOverride == null)
            interactables.Add(gameObject, this);
        else
            interactables.Add(gameobjectOverride, this);

        foreach (MeshRenderer r in GetComponentsInChildren<MeshRenderer>())
        {
            renderMaterialList.Add(r, r.material);
        }

        foreach (Collider col in GetComponentsInChildren<Collider>())
        {
            colliders.Add(col);
        }

        if (colliders.Count > 0)
            mainColliderSize = colliders[0].bounds.size;
        else
            mainColliderSize = Vector3.zero;

        hasRigidBody = TryGetComponent(out rb);
        trans = transform;
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

    public void SetText(string text)
    {
        this.text.text = text;
    }
    public void SetDelegate(ButtonClickedEvent e)
    {
        m_OnClick = e;
    }
}
