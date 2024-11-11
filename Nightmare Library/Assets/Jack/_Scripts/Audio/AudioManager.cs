using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    private static GameObject audioSourceObject;

    // Sounds that will be used
    public List<AudioData> e_Music = new List<AudioData>();

    private Dictionary<string, List<AudioData>> sounds = new Dictionary<string, List<AudioData>>();

    private static ObjectPool soundSourcePool = new ObjectPool();

    private void Awake()
    {
        if(Instance != null)
            Destroy(Instance);
        Instance = this;

        audioSourceObject = PrefabHandler.Instance.a_AudioSource;
        soundSourcePool.PoolObject(audioSourceObject, 20, false);

        sounds.Add("e_Music", e_Music);
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
}
