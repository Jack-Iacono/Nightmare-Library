using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMoveable
{
    // Used for easy referencing
    public static Dictionary<GameObject, IMoveable> instances = new Dictionary<GameObject, IMoveable>();
    public GameObject Pickup();
    public void Place(Vector3 pos, Quaternion rot);
    public void Throw(Vector3 pos, Vector3 force);
}
