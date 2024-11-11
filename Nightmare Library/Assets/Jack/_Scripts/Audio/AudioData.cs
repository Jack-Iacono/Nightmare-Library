using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "AudioData", menuName = "ScriptableObjects/AudioData", order = 1)]
public class AudioData : ScriptableObject
{
    public AudioClip audioClip;

    public bool playOnAwake = false;
    public bool loop = false;

    [Range(0, 256)]
    public float priority = 128;
    [Range(0, 1)]
    public float volume = 1;
    [Range(-3, 3)]
    public float pitch = 1;
    [Range(-1, 1)]
    public float stereoPan = 0;
    [Range(0, 1)]
    public float spatialBlend = 1;
    [Range(0, 1.1f)]
    public float reverbZoneMix = 1;

    [Header("3D Sound Settings")]
    [Range(0, 5)]
    public float dopplerLevel = 1;
    [Range(0, 360)]
    public float spread = 0;

    [Header("Rolloff")]
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;
    public float minDistance = 0;
    public float maxDistance = 20;
    public AnimationCurve rollOffCurve;
}
