using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyFlickerable
{
    // Used for easy referencing
    public static Dictionary<GameObject, IEnemyFlickerable> instances = new Dictionary<GameObject, IEnemyFlickerable>();
    public void Flicker();
}
