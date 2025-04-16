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

    public GameObject loadingScreen;
    private static List<Scene> busyScenes = new List<Scene>();
    private static bool loadBusy = false;

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

    public delegate void OnLoadDelegate();
    public static event OnLoadDelegate OnBeginLoad;
    public static event OnLoadDelegate OnEndLoad;

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
        Debug.Log($"Scene {s.name} loaded");
        // If the scene loaded was a map scene, set it as active
        if(mapScenes.Contains(s.name))
            SceneManager.SetActiveScene(s);

        RemoveBusyScene(s);
        CheckMapLoaded();
    }
    private void OnSceneUnloaded(Scene s)
    {
        RemoveBusyScene(s);
    }

    public static void AddBusyScene(Scene s)
    {
        busyScenes.Add(s);
        CheckBusyScenes();
    }
    public static void RemoveBusyScene(Scene s)
    {
        busyScenes.Remove(s);
        CheckBusyScenes();
    }
    private static void CheckBusyScenes()
    {
        if(busyScenes.Count > 0)
        {
            SetLoadScreen(true);
            if (!loadBusy)
            {
                loadBusy = true;
                OnBeginLoad?.Invoke();
            }
        }
        else
        {
            SetLoadScreen(false);
            if (loadBusy)
            {
                loadBusy = false;
                OnEndLoad?.Invoke();
            }
        } 
    }

    public static void SetLoadScreen(bool b)
    {
        Debug.Log("Show Screen " + b);
        instance.loadingScreen.SetActive(b);
    }

    protected static void UnloadScene(m_Scene scene, bool offlineOverride = false)
    {
        UnloadScene(scenes[scene].name, offlineOverride);
    }
    protected static void UnloadScene(string scene, bool offlineOverride = false)
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

            AddBusyScene(SceneManager.GetSceneByName(scene));
        }
    }

    protected static void LoadScene(m_Scene scene, bool offlineOverride = false)
    {
        LoadScene(scenes[scene].name, offlineOverride);
    }
    protected static void LoadScene(string scene, bool offlineOverride = false)
    {
        if (offlineOverride || !NetworkConnectionController.connectedToLobby)
        {
            SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        }
        else
        {
            OnAsyncLoad?.Invoke(scene);
        }

        AddBusyScene(SceneManager.GetSceneByName(scene));
    }

    public static void SetScenes(List<m_Scene> list)
    {
        List<string> unloadScenes = new List<string>();
        List<string> loadScenes = new List<string>();

        List<string> loadedNames = new List<string>();

        // Get the list of loaded scenes for easier use
        for(int i = 0; i < SceneManager.loadedSceneCount; i++)
        {
            loadedNames.Add(SceneManager.GetSceneAt(i).name);
        }

        // run through the scenes and decide which ones to load vs unload
        foreach(m_Scene s in scenes.Keys)
        {
            // Load the scene if not present in loaded names, but is present in the list provided
            if(list.Contains(s) && !loadedNames.Contains(scenes[s].name))
            {
                // Load the map first
                if (scenes[s].type == SceneData.Type.MAP)
                    loadScenes.Insert(0, scenes[s].name);
                else
                    loadScenes.Add(scenes[s].name);
            }
            // Unload the scene if present in the loaded names, but not present in the list provided
            else if (!list.Contains(s) && loadedNames.Contains(scenes[s].name))
            {
                unloadScenes.Add(scenes[s].name);
            }
        }

        foreach(string uScene in unloadScenes)
        {
            UnloadScene(uScene);
        }
        foreach(string lScene in loadScenes)
        {
            LoadScene(lScene);
        }
    }
    public static void LoadMainMenuScene()
    {
        SceneController.SetScenes(new List<SceneController.m_Scene>()
        {
            SceneController.m_Scene.MAIN_MENU,
        });
    }
    public static void LoadPregameScene()
    {
        SceneController.SetScenes(new List<SceneController.m_Scene>()
        {
            SceneController.m_Scene.PREGAME,
            SceneController.m_Scene.UNIVERSAL
        });
    }
    public static void LoadGameScene()
    {
        SceneController.SetScenes(new List<SceneController.m_Scene>()
        {
            SceneController.m_Scene.GAME,
            SceneController.m_Scene.GAME_SYS,
            SceneController.m_Scene.UNIVERSAL
        });
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
