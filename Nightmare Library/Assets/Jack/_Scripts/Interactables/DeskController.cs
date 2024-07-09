using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeskController : MonoBehaviour
{
    public static PlayerController[] playersAtDesk;

    // Start is called before the first frame update
    void Start()
    {
        playersAtDesk = PlayerController.playerInstances.ToArray();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
