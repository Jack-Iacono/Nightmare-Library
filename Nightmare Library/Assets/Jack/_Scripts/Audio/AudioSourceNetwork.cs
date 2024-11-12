using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(AudioSourceController))]
public class AudioSourceNetwork : NetworkBehaviour
{
    AudioSourceController parent;
    private void Awake()
    {
        parent = GetComponent<AudioSourceController>();
    }

}
