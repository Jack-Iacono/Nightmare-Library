using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyHystericObject
{
    // Used for easy referencing
    public static Dictionary<GameObject, IEnemyHystericObject> instances = new Dictionary<GameObject, IEnemyHystericObject>();
    public void ExecuteHystericInteraction(Enemy user);
}
