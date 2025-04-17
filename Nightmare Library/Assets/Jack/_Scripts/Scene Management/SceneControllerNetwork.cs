using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using static SceneController;

public class SceneControllerNetwork : NetworkBehaviour
{
    public static SceneControllerNetwork instance { get; private set; }
    private SceneController parent;

    public static event EventHandler<Scene> OnSceneUnload;
    public static event EventHandler<Scene> OnSceneLoad;

    private bool eventInProgress = false;
    private List<BufferItem> sceneBuffer = new List<BufferItem>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);

        parent = GetComponent<SceneController>();

        SceneController.OnAsyncLoad += OnLoadScene;
        SceneController.OnAsyncUnload += OnUnloadScene;
    }

    public override void OnNetworkSpawn()
    {
        if(instance == this)
            NetworkManager.SceneManager.OnSceneEvent += SceneManager_OnSceneEvent;

        NetworkSceneManager m = NetworkManager.SceneManager;

        if (NetworkManager.IsServer)
        {
            m.SetClientSynchronizationMode(LoadSceneMode.Additive);
            m.ActiveSceneSynchronizationEnabled = true;

            SceneController.OnSceneTargetChange += OnSceneTargetChanged;
        }
        else
        {
            m.PostSynchronizationSceneUnloading = true;
        }

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
                //Debug.Log($"Loaded the {sceneEvent.SceneName} scene on {clientOrServer}-({sceneEvent.ClientId}).");
                Debug.Log("Scene Loaded");
                break;
            case SceneEventType.UnloadComplete:
                //Debug.Log($"Unloaded the {sceneEvent.SceneName} scene on {clientOrServer}-({sceneEvent.ClientId}).");
                break;
            case SceneEventType.LoadEventCompleted:
                //Debug.Log($"Load event completed for the following client identifiers:({sceneEvent.ClientsThatCompleted})");
                if (clientOrServer == 1)
                {
                    CheckSceneBuffer();
                }
                break;
            case SceneEventType.UnloadEventCompleted:
                //Debug.Log($"Unload event completed for the following client identifiers:({sceneEvent.ClientsThatCompleted})");
                if (clientOrServer == 1)
                {
                    CheckSceneBuffer();
                }
                break;
        }
    }

    public void LoadScene(string s)
    {
        // Check if you are running the server and if the scene to be loaded is not null
        if (IsServer && !string.IsNullOrEmpty(s))
        {
            eventInProgress = true;

            OnSceneLoad?.Invoke(this, SceneManager.GetSceneByName(s));
            var status = NetworkManager.SceneManager.LoadScene(s, LoadSceneMode.Additive);
            CheckStatus(status);
        }
    }
    public void UnloadScene(string s)
    {
        // Assure only the server calls this when the NetworkObject is spawned and the scene is loaded.
        //Debug.Log($"Loaded Scene: {loadedScene.name}  || Unload Buffer: {unloadBuffer.name}");
        //Debug.Log((!IsServer).ToString() + " || " + (!IsSpawned).ToString() + " || " + (!sceneBuffer.IsValid()).ToString() + " || " + (!sceneBuffer.isLoaded).ToString());

        if (IsServer && !string.IsNullOrEmpty(s))
        {
            eventInProgress = true;

            // Unload the scene
            OnSceneUnload?.Invoke(this, SceneManager.GetSceneByName(s));
            var status = NetworkManager.SceneManager.UnloadScene(SceneManager.GetSceneByName(s));
            CheckStatus(status, false);
        }
    }

    private void OnLoadScene(string s)
    {
        if (!eventInProgress)
            LoadScene(s);
        else
            sceneBuffer.Add(new BufferItem(s, true));
    }
    private void OnUnloadScene(string s)
    {
        if (!eventInProgress)
            UnloadScene(s);
        else
            sceneBuffer.Add(new BufferItem(s, false));
    }

    private void CheckSceneBuffer()
    {
        if (sceneBuffer.Count > 0)
        {
            if (sceneBuffer[0].loadUnload)
                LoadScene(sceneBuffer[0].scene);
            else
                UnloadScene(sceneBuffer[0].scene);

            sceneBuffer.RemoveAt(0);
        }
        else
            eventInProgress = false;
    }

    private void OnSceneTargetChanged(List<m_Scene> list, bool fromNetwork = false)
    {
        if (!fromNetwork)
        {
            int[] scenes = new int[list.Count];
            for (int i = 0; i < scenes.Length; i++)
            {
                scenes[i] = (int)list[i];
            }
            OnSceneTargetChangedClientRpc(scenes);
        }
    }
    [ClientRpc]
    private void OnSceneTargetChangedClientRpc(int[] list)
    {
        if (!NetworkManager.IsServer)
        {
            List<m_Scene> scenes = new List<m_Scene>();
            foreach (int i in list)
            {
                scenes.Add((m_Scene)i);
            }
            SceneController.SetSceneTarget(scenes, true);
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        SceneController.OnAsyncLoad -= OnLoadScene;
        SceneController.OnAsyncUnload -= OnUnloadScene;
    }
    private class BufferItem
    {
        public string scene;
        public bool loadUnload;

        public BufferItem(string scene, bool loadUnload)
        {
            this.scene = scene;
            this.loadUnload = loadUnload;
        }
    }
}
