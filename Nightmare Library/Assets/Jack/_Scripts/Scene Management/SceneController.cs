using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;
using System.Linq;

public class SceneController : MonoBehaviour
{
    public static SceneController instance { get; private set; }

    public enum m_Scene { MAIN_MENU, GAME, PREGAME, UNIVERSAL, GAME_SYS };
    public readonly static Dictionary<m_Scene, SceneData> scenes = new Dictionary<m_Scene, SceneData>
        {
            { m_Scene.MAIN_MENU, new SceneData("j_Menu", SceneData.Type.MAP) },
            { m_Scene.GAME, new SceneData("j_Game", SceneData.Type.MAP) },
            { m_Scene.PREGAME, new SceneData("j_PreGame", SceneData.Type.MAP) },
            { m_Scene.UNIVERSAL, new SceneData("scn_UniversalGameFunction", SceneData.Type.UTIL) },
            { m_Scene.GAME_SYS, new SceneData("scn_GameSystems", SceneData.Type.UTIL) }
        };
    private static List<string> mapScenes = new List<string>();

    public delegate void OnAsyncLoadDelegate(string scene);
    public static event OnAsyncLoadDelegate OnAsyncLoad;

    public delegate void OnAsyncUnloadDelegate(string scene);
    public static event OnAsyncUnloadDelegate OnAsyncUnload;

    public delegate void OnMapLoadedDelegate(string mapName);
    public static event OnMapLoadedDelegate OnMapLoaded;
    public static Scene loadedMap;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            foreach(SceneData s in scenes.Values)
            {
                if(s.type == SceneData.Type.MAP)
                {
                    mapScenes.Add(s.name);
                }
            }
        }
        else
            Destroy(this);
    }

    private void Start()
    {
        LoadScene(m_Scene.MAIN_MENU);
    }

    private void OnSceneLoaded(Scene s, LoadSceneMode loadMode)
    {
        //Debug.Log($"Scene {s.name} loaded");
        // If the scene loaded was a map scene, set it as active
        if(mapScenes.Contains(s.name))
            SceneManager.SetActiveScene(s);

        CheckMapLoaded();
    }
    private void OnSceneUnloaded(Scene s)
    {
        //Debug.Log($"Scene {s.name} unloaded");
    }

    public static void UnloadScene(m_Scene scene, bool offlineOverride = false)
    {
        UnloadScene(scenes[scene].name, offlineOverride);
    }
    public static void UnloadScene(string scene, bool offlineOverride = false)
    {
        if (SceneManager.GetSceneByName(scene).isLoaded)
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
    }

    public static void LoadScene(m_Scene scene, bool offlineOverride = false)
    {
        LoadScene(scenes[scene].name, offlineOverride);
    }
    public static void LoadScene(string scene, bool offlineOverride = false)
    {
        Debug.Log("Load Scene " + scene);

        if (offlineOverride || !NetworkConnectionController.connectedToLobby)
        {
            SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        }
        else
        {
            OnAsyncLoad?.Invoke(scene);
        }
    }

    /// <summary>
    /// Checks to see if there is a map scene currently loaded
    /// </summary>
    private void CheckMapLoaded()
    {
        // Run through the scenes that are known by the SceneManager
        for(int i = 0; i < SceneManager.sceneCount; i++)
        {
            // Check if there is any map loaded, and if there is, send event
            if (mapScenes.Contains(SceneManager.GetSceneAt(i).name) && SceneManager.GetSceneAt(i).isLoaded)
            {
                loadedMap = SceneManager.GetSceneAt(i);
                OnMapLoaded?.Invoke(loadedMap.name);
                break;
            }
        }
    }

    public static bool GetSceneLoaded(m_Scene scene)
    {
        for(int i = 0; i < SceneManager.sceneCount;i++)
        {
            if (SceneManager.GetSceneAt(i) == SceneManager.GetSceneByName(scenes[scene].name))
                return true;
        }
        return false;
    }

    public static void SetMapActive()
    {
        if(loadedMap.isLoaded)
            SceneManager.SetActiveScene(loadedMap);
    }
    public static void SetSceneActive(m_Scene scene)
    {
        if(SceneManager.GetSceneByName(scenes[scene].name).isLoaded)
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(scenes[scene].name));
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;   
    }

    public class SceneData
    {
        public string name;
        
        public enum Type { MAP, UTIL }
        public Type type;

        public SceneData(string name, Type type)
        {
            this.name = name;
            this.type = type;
        }
    }
}
