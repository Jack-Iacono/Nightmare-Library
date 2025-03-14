using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyThrowable
{
    // Used for easy referencing
    public static Dictionary<GameObject, IEnemyThrowable> instances = new Dictionary<GameObject, IEnemyThrowable>();
    public void Launch();
}
