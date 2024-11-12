using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEditor;
using UnityEngine;
using static AudioManager;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    private static GameObject audioSourceObject;

    public enum SoundType
    {
        e_MUSIC_LOVER,
        e_STALK_CLOSE_IN
    }

    // Sounds that will be used
    public SoundList[] sounds = new SoundList[0];
    // Use this for telling networked objects which sounds to use over the network
    public Dictionary<string, AudioData> audioReference = new Dictionary<string, AudioData>();

    private static ObjectPool soundSourcePool = new ObjectPool();

    private void Awake()
    {
        if(Instance != null)
            Destroy(Instance);
        Instance = this;

        // Sets up the audio dictionary
        for(int i = 0; i < sounds.Length; i++)
        {
            for(int j = 0; j < sounds[i].sounds.Length; j++)
            {
                audioReference.Add(sounds[i].sounds[j].audioClip.name, sounds[i].sounds[j]);
            }
        }

        audioSourceObject = PrefabHandler.Instance.a_AudioSource;
        soundSourcePool.PoolObject(audioSourceObject, 20, false);
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
