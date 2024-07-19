using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEditor.SceneManagement;
using System;

public class OfflineSceneController : MonoBehaviour
{
    public static OfflineSceneController instance { get; private set; }

    public static Scene loadedScene;

    public enum m_Scene { MAIN_MENU, OFFLINE_GAME, ONLINE_GAME };
    public readonly static Dictionary<m_Scene, int> scenes = new Dictionary<m_Scene, int>
        {
            { m_Scene.MAIN_MENU, 0 },
            { m_Scene.OFFLINE_GAME, 1 },
            { m_Scene.ONLINE_GAME, 2 }
        };

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

    public static void ChangeScene(m_Scene scene)
    {
        ChangeScene(scenes[scene]);
    }
    public static void ChangeScene(int scene)
    {
        SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
    }

    private static m_Scene FindSceneByIndex(int index)
    {
        foreach(m_Scene scene in scenes.Keys)
        {
            if (scenes[scene] == index) return scene;
        }

        return m_Scene.MAIN_MENU;
    }
    public static string GetSceneName(int index)
    {
        return SceneManager.GetSceneByBuildIndex(index).name;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
        SceneManager.sceneUnloaded -= OnSceneUnload;
    }
}
