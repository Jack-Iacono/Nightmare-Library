using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    // Used for easy referencing
    public static Dictionary<GameObject, IInteractable> instances = new Dictionary<GameObject, IInteractable>();
    public void Interact();
}
