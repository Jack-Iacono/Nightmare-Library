using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneControllerNetwork : NetworkBehaviour
{
    public static SceneControllerNetwork instance { get; private set; }
    private SceneController parent;

    public static event EventHandler<Scene> OnSceneUnload;
    public static event EventHandler<Scene> OnSceneLoad;

    private Scene sceneBuffer;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);

        parent = GetComponent<SceneController>();

        SceneController.OnChangeScene += OnChangeScene;

        Debug.Log(name + ": " + GetHashCode().ToString());
    }

    public override void OnNetworkSpawn()
    {
        if(instance == this)
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
        int clientOrServer = sceneEvent.ClientId == NetworkManager.ServerClientId ? 1 : 0;
        
        switch (sceneEvent.SceneEventType)
        {
            case SceneEventType.LoadComplete:
                // We want to handle this for only the server-side
                if (clientOrServer == 1)
                {
                    sceneBuffer = SceneController.loadedScene;
                    SceneController.loadedScene = sceneEvent.Scene;
                    //Debug.Log($"Loaded Scene: {SceneController.loadedScene.name}  || Unload Buffer: {sceneBuffer.name}");
                }
                else if (!NetworkManager.IsServer)
                {
                    SceneController.loadedScene = sceneEvent.Scene;
                }
                //Debug.Log($"Loaded the {sceneEvent.SceneName} scene on {clientOrServer}-({sceneEvent.ClientId}).");
                break;
            case SceneEventType.UnloadComplete:
                //Debug.Log($"Unloaded the {sceneEvent.SceneName} scene on {clientOrServer}-({sceneEvent.ClientId}).");
                break;
            case SceneEventType.LoadEventCompleted:
                Debug.Log($"Load event completed for the following client identifiers:({sceneEvent.ClientsThatCompleted})");

                UnloadScene();

                if (sceneEvent.ClientsThatTimedOut.Count > 0)
                    Debug.LogWarning($"Load event timed out for the following client identifiers:({sceneEvent.ClientsThatTimedOut})");

                break;
            case SceneEventType.UnloadEventCompleted:
                //Debug.Log($"Unload event completed for the following client identifiers:({sceneEvent.ClientsThatCompleted})");

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
            OnSceneLoad?.Invoke(this, SceneManager.GetSceneByName(s));
            var status = NetworkManager.SceneManager.LoadScene(s, LoadSceneMode.Additive);
            //CheckStatus(status);
        }
    }
    public void UnloadScene()
    {
        // Assure only the server calls this when the NetworkObject is
        // spawned and the scene is loaded.
        //Debug.Log($"Loaded Scene: {loadedScene.name}  || Unload Buffer: {unloadBuffer.name}");
        //Debug.Log(OfflineSceneController.loadedScene.name);
        //Debug.Log(!IsServer + " || " + !IsSpawned + " || " + !OfflineSceneController.loadedScene.IsValid() + " || " + !OfflineSceneController.loadedScene.isLoaded);
        if (!IsServer || !IsSpawned || !sceneBuffer.IsValid() || !sceneBuffer.isLoaded)
        {
            return;
        }

        // Unload the scene
        OnSceneUnload?.Invoke(this, sceneBuffer);
        var status = NetworkManager.SceneManager.UnloadScene(SceneManager.GetSceneByName(sceneBuffer.name));
        //CheckStatus(status, false);
    }

    private void OnChangeScene(object sender, string s)
    {
        LoadScene(s);
    }
}
