using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioSourceController : MonoBehaviour
{
    public static Dictionary<GameObject, AudioSourceController> sourceAccess = new Dictionary<GameObject, AudioSourceController>();
    private AudioSource audioSource;
    private Transform trans;

    private bool isPlaying = false;
    private float playTimer = 0;

    public bool isPooled = false;

    private AudioData audioData;

    public delegate void OnPlayDelegate(AudioData sound = null, bool move = false);
    public event OnPlayDelegate OnPlay;

    public delegate void OnProjectDelegate(Vector3 pos, float radius = 10);
    public static event OnProjectDelegate OnProject;

    public bool checkListeners = true;

    private void Awake()
    {
        sourceAccess.Add(gameObject, this);
        audioSource = GetComponent<AudioSource>();
        trans = transform;
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

    public void Pool()
    {
        isPooled = true;
        AudioManager.soundSourcePool.AddObjectToPool(PrefabHandler.Instance.a_AudioSource, gameObject);
        gameObject.SetActive(false);
    }

    public void PlaySound(bool fromNetwork = false)
    {
        gameObject.SetActive(true);
        audioSource.Play();
        BeginPlayTimer();

        // Only sends this out if on the server, moderated by the AudioSourceNetwork
        if (checkListeners)
            OnProject?.Invoke(trans.position);

        // Send all data to ensure correct sound is played
        if (!fromNetwork)
            OnPlay?.Invoke(audioData, true);
    }
    public void PlaySound(AudioData sound, bool fromNetwork = false)
    {
        SetAudioSourceData(sound);
        PlaySound(fromNetwork);
    }
    public void PlaySound(AudioData sound, Vector3 pos, bool fromNetwork = false)
    {
        trans.position = pos;
        PlaySound(sound, fromNetwork);
    }

    public void PlaySoundOffline()
    {
        gameObject.SetActive(true);
        audioSource.Play();
        BeginPlayTimer();
    }
    public void PlaySoundOffline(AudioData sound)
    {
        SetAudioSourceData(sound);
        PlaySoundOffline();
    }
    public void PlaySoundOffline(AudioData sound, Vector3 pos)
    {
        trans.position = pos;
        PlaySoundOffline(sound);
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

    private void OnDestroy()
    {
        sourceAccess.Remove(gameObject);
    }
}
