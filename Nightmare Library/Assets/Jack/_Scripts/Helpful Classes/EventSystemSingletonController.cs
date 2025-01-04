using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSystemSingletonController : MonoBehaviour
{
    private static EventSystemSingletonController Instance;

    private void Awake()
    {
        if(Instance != null && Instance.gameObject != null)
            Destroy(Instance.gameObject);
        Instance = this;
    }
}
