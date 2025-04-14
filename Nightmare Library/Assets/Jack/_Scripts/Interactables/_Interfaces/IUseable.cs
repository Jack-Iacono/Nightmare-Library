using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUseable
{
    public static Dictionary<GameObject, IUseable> Instances = new Dictionary<GameObject, IUseable>();

    public void Use();
}
