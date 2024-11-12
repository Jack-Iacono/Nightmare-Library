using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioSourceController : MonoBehaviour
{
    public static Dictionary<GameObject, AudioSourceController> sourceAccess = new Dictionary<GameObject, AudioSourceController>();
    private AudioSource audioSource;
    private Transform trans;

    private AudioData audioData;

    private void Awake()
    {
        sourceAccess.Add(gameObject, this);
        audioSource = GetComponent<AudioSource>();
        trans = transform;
    } 
    private void OnDestroy()
    {
        sourceAccess.Remove(gameObject);
    }

    public void PlaySound()
    {
        audioSource.Play();
    }
    public void PlaySound(AudioData sound)
    {
        SetAudioSourceData(sound);
        audioSource.Play();
    }
    public void PlaySound(AudioData sound, Vector3 pos)
    {
        trans.position = pos;
        SetAudioSourceData(sound);
        audioSource.Play();
    }

    private void SetAudioSourceData(AudioData sound)
    {
        audioData = sound;

        audioSource.clip = sound.audioClip;
        audioSource.playOnAwake = sound.playOnAwake;
        audioSource.loop = sound.loop;

        audioSource.priority = sound.priority;
        audioSource.volume = sound.volume;
        audioSource.pitch = sound.pitch;
        audioSource.panStereo = sound.stereoPan;
        audioSource.spatialBlend = sound.spatialBlend;
        audioSource.reverbZoneMix = sound.reverbZoneMix;

        audioSource.dopplerLevel = sound.dopplerLevel;
        audioSource.spread = sound.spread;

        audioSource.rolloffMode = sound.rolloffMode;
        audioSource.minDistance = sound.minDistance;
        audioSource.maxDistance = sound.maxDistance;
        if(sound.rolloffMode == AudioRolloffMode.Custom)
            audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, sound.rollOffCurve);
    }
}
