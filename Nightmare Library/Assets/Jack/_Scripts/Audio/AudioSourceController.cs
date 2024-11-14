using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioSourceController : MonoBehaviour
{
    public static Dictionary<GameObject, AudioSourceController> sourceAccess = new Dictionary<GameObject, AudioSourceController>();
    private AudioSource audioSource;
    private Transform trans;

    private bool isPlaying = false;
    private float playTimer = 0;

    private bool isPooled = false;

    private AudioData audioData;

    public delegate void OnPlayDelegate(AudioData sound = null, bool move = false);
    public event OnPlayDelegate OnPlay;

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

    public void Initialize()
    {
        isPooled = true;
    }

    private void Update()
    {
        if (isPooled && isPlaying)
        {
            if(playTimer > 0)
                playTimer -= Time.deltaTime;
            else
            {
                isPlaying = false;
                gameObject.SetActive(false);
            }
        }
    }

    public void PlaySound(bool fromNetwork = false)
    {
        gameObject.SetActive(true);
        audioSource.Play();
        BeginPlayTimer();

        if (!fromNetwork)
            OnPlay?.Invoke();
    }
    public void PlaySound(AudioData sound, bool fromNetwork = false)
    {
        gameObject.SetActive(true);
        SetAudioSourceData(sound);
        audioSource.Play();
        BeginPlayTimer();

        if (!fromNetwork)
            OnPlay?.Invoke(sound);
    }
    public void PlaySound(AudioData sound, Vector3 pos, bool fromNetwork = false)
    {
        gameObject.SetActive(true);
        trans.position = pos;
        SetAudioSourceData(sound);
        audioSource.Play();
        BeginPlayTimer();

        if (!fromNetwork)
            OnPlay?.Invoke(sound, true);
    }

    private void BeginPlayTimer()
    {
        isPlaying = true;
        playTimer = audioData.clipLength;
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
