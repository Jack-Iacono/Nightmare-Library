using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEditor.SceneManagement;
using System;

public class SceneController : MonoBehaviour
{
    public static SceneController instance { get; private set; }

    public static Scene loadedScene;

    public enum m_Scene { MAIN_MENU, OFFLINE_GAME, ONLINE_GAME };
    public readonly static Dictionary<m_Scene, string> scenes = new Dictionary<m_Scene, string>
        {
            { m_Scene.MAIN_MENU, "j_Menu" },
            { m_Scene.OFFLINE_GAME, "j_OfflineGame" },
            { m_Scene.ONLINE_GAME, "j_OnlineGame" }
        };

    public static event EventHandler<string> OnChangeScene;

    public static bool SceneIsLoaded
    {
        get
        {
            if (loadedScene.IsValid() && loadedScene.isLoaded)
            {
                return true;
            }
            return false;
        }
    }

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(this);
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
        SceneManager.sceneUnloaded += OnSceneUnload;

        loadedScene = SceneManager.GetActiveScene();
    }

    private void OnSceneLoad(Scene s, LoadSceneMode loadMode)
    {
        Debug.Log($"Scene {s.name} loaded");

        if (!NetworkConnectionController.IsOnline)
        {
            if (loadMode == LoadSceneMode.Additive)
            {
                SceneManager.UnloadSceneAsync(loadedScene);
                loadedScene = s;
            }
        }
    }
    private void OnSceneUnload(Scene s)
    {
        Debug.Log($"Scene {s.name} unloaded");
    }

    public static void LoadScene(m_Scene scene)
    {
        LoadScene(scenes[scene]);
    }
    public static void LoadScene(string scene)
    {
        Debug.Log(!NetworkConnectionController.instance.isRunning);
        if (!NetworkConnectionController.instance.isRunning)
        {
            SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        }
        else
        {
            OnChangeScene?.Invoke(instance, scene);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
        SceneManager.sceneUnloaded -= OnSceneUnload;
    }
}
