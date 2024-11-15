using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(AudioManager))]
public class AudioManagerNetwork : NetworkBehaviour
{
    private AudioManager parent;

    private void Awake()
    {
        parent = GetComponent<AudioManager>();
    }


}
