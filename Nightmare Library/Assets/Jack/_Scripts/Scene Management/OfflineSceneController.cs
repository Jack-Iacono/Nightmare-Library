using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEditor.SceneManagement;

public static class OfflineSceneController
{
    public static m_Scene currentScene;

    private static Scene loadedScene;
    private static Scene unloadSceneBuffer;

    public enum m_Scene { MAIN_MENU, OFFLINE_GAME, ONLINE_GAME };
    public static Dictionary<m_Scene, int> scenes = new Dictionary<m_Scene, int>
        {
            { m_Scene.MAIN_MENU, 0 },
            { m_Scene.OFFLINE_GAME, 1 },
            { m_Scene.ONLINE_GAME, 2 }
        };

    public static void ChangeScene(m_Scene scene)
    {
        SceneManager.LoadScene(scenes[scene], LoadSceneMode.Additive);
        currentScene = scene;
    }
    public static void ChangeScene(int scene)
    {
        SceneManager.LoadScene(scene);
        currentScene = FindSceneByIndex(scene);
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
}
