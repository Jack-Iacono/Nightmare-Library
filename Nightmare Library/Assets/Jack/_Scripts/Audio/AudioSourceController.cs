using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioSourceController : MonoBehaviour
{
    public static Dictionary<GameObject, AudioSourceController> sourceAccess = new Dictionary<GameObject, AudioSourceController>();
    private AudioSource audioSource;
    private Transform trans;

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

    public void PlaySound(AudioData sound)
    {
        audioSource.clip = sound.audioClip;
    }
    public void PlaySound(AudioData sound, Vector3 pos)
    {
        trans.position = pos;
        PlaySound(sound);
    }
}
