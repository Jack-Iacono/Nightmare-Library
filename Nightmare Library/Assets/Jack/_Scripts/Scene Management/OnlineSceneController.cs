using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OnlineSceneController : NetworkBehaviour
{
    public static OnlineSceneController instance { get; private set; }

    [SerializeField]
    private string[] sceneNames = { "j_Menu", "j_OnlineGame", "j_OfflineGame" };

    private static Scene loadedScene;
    private static Scene unloadBuffer;

    public bool SceneIsLoaded
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
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    public override void OnNetworkSpawn()
    {
        loadedScene = SceneManager.GetActiveScene();

        NetworkManager.SceneManager.OnSceneEvent += SceneManager_OnSceneEvent;

        base.OnNetworkSpawn();
    }

    private void CheckStatus(SceneEventProgressStatus status, bool isLoading = true)
    {
        var sceneEventAction = isLoading ? "load" : "unload";
        if (status != SceneEventProgressStatus.Started)
        {
            Debug.LogWarning($"Failed to {sceneEventAction} scene with a {nameof(SceneEventProgressStatus)}: {status}");
        }
    }

    /// <summary>
    /// Handles processing notifications when subscribed to OnSceneEvent
    /// </summary>
    /// <param name="sceneEvent">class that has information about the scene event</param>
    private void SceneManager_OnSceneEvent(SceneEvent sceneEvent)
    {
        // Used for denoting in warning message
        var clientOrServer = sceneEvent.ClientId == NetworkManager.ServerClientId ? "server" : "client";
        
        switch (sceneEvent.SceneEventType)
        {
            case SceneEventType.LoadComplete:
                // We want to handle this for only the server-side
                if (sceneEvent.ClientId == NetworkManager.ServerClientId)
                {
                    // *** IMPORTANT ***
                    // Keep track of the loaded scene, you need this to unload it
                    if (loadedScene != null)
                        unloadBuffer = loadedScene;

                    loadedScene = sceneEvent.Scene;

                    Debug.Log($"Loaded Scene: {loadedScene.name}  || Unload Buffer: {unloadBuffer.name}");
                }
                Debug.Log($"Loaded the {sceneEvent.SceneName} scene on {clientOrServer}-({sceneEvent.ClientId}).");
                break;
            case SceneEventType.UnloadComplete:
                Debug.Log($"Unloaded the {sceneEvent.SceneName} scene on {clientOrServer}-({sceneEvent.ClientId}).");
                break;
            case SceneEventType.LoadEventCompleted:
                Debug.Log($"Load event completed for the following client identifiers:({sceneEvent.ClientsThatCompleted})");
                Debug.Log("Unload");

                if(unloadBuffer != null)
                    UnloadScene();

                if (sceneEvent.ClientsThatTimedOut.Count > 0)
                    Debug.LogWarning($"Load event timed out for the following client identifiers:({sceneEvent.ClientsThatTimedOut})");

                break;
            case SceneEventType.UnloadEventCompleted:
                Debug.Log($"Unload event completed for the following client identifiers:({sceneEvent.ClientsThatCompleted})");

                if (sceneEvent.ClientsThatTimedOut.Count > 0)
                    Debug.LogWarning($"Unload event timed out for the following client identifiers:({sceneEvent.ClientsThatTimedOut})");

                break;
        }
    }

    public void LoadScene(string s)
    {
        // Check if you are running the server and if the scene to be loaded is not null
        if (IsServer && !string.IsNullOrEmpty(s))
        {
            var status = NetworkManager.SceneManager.LoadScene(s, LoadSceneMode.Additive);
            CheckStatus(status);
        }
    }
    public void UnloadScene()
    {
        // Assure only the server calls this when the NetworkObject is
        // spawned and the scene is loaded.
        Debug.Log($"Loaded Scene: {loadedScene.name}  || Unload Buffer: {unloadBuffer.name}");
        Debug.Log(!IsServer + " || " + !IsSpawned + " || " + !loadedScene.IsValid() + " || " + !loadedScene.isLoaded);
        if (!IsServer || !IsSpawned || !loadedScene.IsValid() || !loadedScene.isLoaded)
        {
            return;
        }

        // Unload the scene
        //var status = NetworkManager.SceneManager.UnloadScene(loadedScene);
        var status = NetworkManager.SceneManager.UnloadScene(SceneManager.GetSceneByName(unloadBuffer.name));
        CheckStatus(status, false);
    }

}
