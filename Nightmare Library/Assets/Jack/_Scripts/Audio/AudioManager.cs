using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using static AudioManager;

public class AudioManager : NetworkBehaviour
{
    public static AudioManager Instance;
    private static GameObject audioSourceObject;

    public enum SoundType
    {
        e_MUSIC_LOVER,
        e_STALK_CLOSE_IN,
        e_SCREECH_APPEAR,
        e_WARDEN_SENSOR_STEP,
        e_STALK_APPEAR,
        p_JUMP
    }

    // Sounds that will be used
    public SoundList[] sounds = new SoundList[0];

    // Used for referencing audio clips over the network
    public static Dictionary<AudioData, Vector2> audioReference = new Dictionary<AudioData, Vector2>();
    public static ObjectPool soundSourcePool = new ObjectPool();

    public delegate void OnPoolObjectsDelegate();
    public static event OnPoolObjectsDelegate OnPoolObjects;

    private void Awake()
    {
        // Singleton
        if(Instance != null)
            Destroy(Instance);
        Instance = this;

        audioReference = new Dictionary<AudioData, Vector2>();

        // Sets the audio data's network reference
        for (int i = 0; i < sounds.Length; i++)
        {
            for(int j = 0; j < sounds[i].sounds.Length; j++)
            {
                // Only take the first instance of that sound
                if(!audioReference.ContainsKey(sounds[i].sounds[j]))
                    audioReference.Add(sounds[i].sounds[j], new Vector2(i, j));
            }
        }
    }
    private void Start()
    {
        audioSourceObject = PrefabHandler.Instance.a_AudioSource;

        if (!NetworkConnectionController.connectedToLobby || NetworkManager.IsServer)
        {
            soundSourcePool.PoolObject(audioSourceObject, 20);
        }
        else
        {
            Debug.Log("Sending Pool Request");
            OnPoolObjects?.Invoke();
        }
    }

    /// <summary>
    /// Plays a sound at the given point using the given sound data
    /// </summary>
    /// <param name="sound">The sound data that should be played</param>
    /// <param name="pos">The position at which to play this sound</param>
    public static void PlaySound(AudioData sound, Vector3 pos)
    {
        GameObject source = soundSourcePool.GetObject(audioSourceObject);
        AudioSourceController.sourceAccess[source].PlaySound(sound, pos);
    }

    public static AudioData GetAudioData(SoundType type)
    {
        AudioData[] s = Instance.sounds[(int)type].sounds;
        return s[UnityEngine.Random.Range(0, s.Length)];
    }
    public static AudioData GetAudioData(int i, int j)
    {
        return Instance.sounds[i].sounds[j];
    }


    public override void OnDestroy()
    {
        base.OnDestroy();
        soundSourcePool.CleanupPool();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        string[] names = Enum.GetNames(typeof(SoundType));
        Array.Resize(ref sounds, names.Length);
        for (int i = 0; i < names.Length; i++)
        {
            sounds[i].name = names[i];
        }
    }
#endif

}

[Serializable]
public struct SoundList
{
    [HideInInspector] public string name;
    [SerializeField] public AudioData[] sounds;
}
