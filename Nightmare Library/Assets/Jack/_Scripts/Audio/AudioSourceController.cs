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

    public delegate void OnPlayDelegate(AudioData sound);
    public event OnPlayDelegate OnPlay;

    public delegate void OnProjectDelegate(SourceData data);
    public static event OnProjectDelegate OnProject;

    private SourceData sourceData;
    public bool checkListeners = true;

    private void Awake()
    {
        sourceAccess.Add(gameObject, this);
        audioSource = GetComponent<AudioSource>();
        sourceData = new SourceData(gameObject);
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

    public void Play(bool fromNetwork = false)
    {
        gameObject.SetActive(true);
        audioSource.Play();
        BeginPlayTimer();

        // Only sends this out if on the server, moderated by the AudioSourceNetwork
        if (checkListeners)
            OnProject?.Invoke(sourceData);

        if(!fromNetwork)
            OnPlay?.Invoke(audioData);
    }
    public void Play(AudioData data, bool fromNetwork = false)
    {
        SetAudioSourceData(data);
        Play(fromNetwork);
    }

    public void Stop()
    {
        isPlaying = false;
        audioSource.Stop();
    }

    private void BeginPlayTimer()
    {
        isPlaying = true;
        playTimer = audioData.clipLength;
    }
    public void SetAudioSourceData(AudioData sound)
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

        // Change this to use volume later
        sourceData.radius = 10;
    }

    private void OnDestroy()
    {
        sourceAccess.Remove(gameObject);
    }

    public class SourceData
    {
        public GameObject gameObject;
        public Transform transform;
        public float radius;

        public SourceData(GameObject gameObject, float radius = 10)
        {
            this.gameObject = gameObject;
            transform = gameObject.transform;
            this.radius = radius;
        }
        public SourceData(GameObject gameObject, Transform transform, float radius = 10)
        {
            this.gameObject = gameObject;
            this.transform = transform;
            this.radius = radius;
        }
    }
}
