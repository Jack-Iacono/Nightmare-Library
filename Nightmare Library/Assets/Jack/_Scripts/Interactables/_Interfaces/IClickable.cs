using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IClickable
{
    // Used for easy referencing
    public static Dictionary<GameObject, IClickable> instances = new Dictionary<GameObject, IClickable>();

    public delegate void OnClickDelegate(IClickable clickable);
    public event OnClickDelegate OnClick;

    public void Click();
    public void Hover(bool enterExit);
}
