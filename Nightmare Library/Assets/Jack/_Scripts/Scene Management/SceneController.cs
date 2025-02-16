using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;

public class SceneController : MonoBehaviour
{
    public static SceneController instance { get; private set; }

    public enum m_Scene { MAIN_MENU, GAME, PREGAME, UNIVERSAL, GAME_SYS };
    public readonly static Dictionary<m_Scene, string> scenes = new Dictionary<m_Scene, string>
        {
            { m_Scene.MAIN_MENU, "j_Menu" },
            { m_Scene.GAME, "j_Game" },
            { m_Scene.PREGAME, "j_PreGame" },
            { m_Scene.UNIVERSAL, "scn_UniversalGameFunction" },
            { m_Scene.GAME_SYS, "scn_GameSystems" }
        };

    public delegate void OnAsyncLoadDelegate(string scene);
    public static event OnAsyncLoadDelegate OnAsyncLoad;

    public delegate void OnAsyncUnloadDelegate(string scene);
    public static event OnAsyncUnloadDelegate OnAsyncUnload;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoad;
            SceneManager.sceneUnloaded += OnSceneUnload;
        }
        else
            Destroy(this);
    }
    private void Start()
    {
        LoadScene(m_Scene.MAIN_MENU);
    }

    private void OnSceneLoad(Scene s, LoadSceneMode loadMode)
    {
        //Debug.Log($"Scene {s.name} loaded");
        SceneManager.SetActiveScene(s);
    }
    private void OnSceneUnload(Scene s)
    {
        //Debug.Log($"Scene {s.name} unloaded");
    }

    public static void UnloadScene(m_Scene scene, bool offlineOverride = false)
    {
        UnloadScene(scenes[scene], offlineOverride);
    }
    public static void UnloadScene(string scene, bool offlineOverride = false)
    {
        if (offlineOverride || !NetworkConnectionController.connectedToLobby)
        {
            SceneManager.UnloadSceneAsync(scene);
        }
        else
        {
            OnAsyncUnload?.Invoke(scene);
        }
    }

    public static void LoadScene(m_Scene scene, bool offlineOverride = false)
    {
        LoadScene(scenes[scene], offlineOverride);
    }
    public static void LoadScene(string scene, bool offlineOverride = false)
    {
        if (offlineOverride || !NetworkConnectionController.connectedToLobby)
        {
            SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        }
        else
        {
            OnAsyncLoad?.Invoke(scene);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
        SceneManager.sceneUnloaded -= OnSceneUnload;   
    }
}
