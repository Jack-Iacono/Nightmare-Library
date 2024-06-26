using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;
using Unity.VisualScripting;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;

public abstract class LobbyController : NetworkBehaviour
{
    public static LobbyController instance { get; protected set; }
    public static List<MonoBehaviour> observers = new List<MonoBehaviour>();

    public const int MAX_PLAYERS = 12;
    public const int MIN_PLAYERS = 2;

    public delegate void LobbyEnterDelegate(ulong clientId, bool isServer);
    public static event LobbyEnterDelegate OnLobbyEnter;

    protected virtual void Awake()
    {
        if (instance != null)
        {
            Destroy(instance);
            instance = this;
        }
        else
        {
            instance = this;
        }
    }

    #region Connection Methods

    public async virtual Task<bool> StartConnection()
    {
        // Attempts to join a lobby, if that doesn't work, leave
        if (!NetworkConnectionController.connectedToLobby)
        {
            if (await NetworkConnectionController.ConnectToLobby())
                await NetworkConnectionController.StartConnection();
            else
            {
                LeaveLobby();
                return false;
            }
        }

        // Both the server-host and client(s) register the custom named message.
        RegisterCallbacks();

        /*
        if (NetworkManager.Singleton.IsServer)
            ServerEntryAction();
        else
            ClientEntryAction();
        */

        return true;
    }

    protected virtual void ServerEntryAction()
    {
        OnLobbyEnter?.Invoke(NetworkManager.LocalClientId, true);
    }
    protected virtual void ClientEntryAction()
    {
        OnLobbyEnter?.Invoke(NetworkManager.LocalClientId, false);
    }

    #endregion

    #region Lobby Leaving

    IEnumerator CheckClientLeave()
    {
        while (true)
        {
            Debug.Log("Clients Connected: " + NetworkManager.ConnectedClients.Count);
            if (NetworkManager.ConnectedClients.Count == 1)
            {
                StopAllCoroutines();
                LeaveLobby();
            }

            yield return new WaitForSeconds(0.5f);
        }
    }
    [ClientRpc]
    public void LeaveLobbyClientRpc()
    {
        if (!IsServer)
        {
            LeaveLobby();
        }
    }
    public virtual async void LeaveLobby()
    {
        try
        {
            UnRegisterCallbacks();
        }
        catch
        {
            Debug.Log("Failed to Unregister Callbacks");
        }

        if (IsServer && NetworkManager.ConnectedClients.Count != 1)
        {
            LeaveLobbyClientRpc();
            StartCoroutine(CheckClientLeave());
        }
        else
        {
            // TEMPORARY!!!
            Cursor.lockState = CursorLockMode.None;

            await NetworkConnectionController.StopConnection();
            OfflineSceneController.ChangeScene(0);
        }
    }

    #endregion

    #region Callbacks

    protected virtual void RegisterCallbacks()
    {
        NetworkManager.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
    }
    protected virtual void UnRegisterCallbacks()
    {
        NetworkManager.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    protected virtual void OnClientConnected(ulong obj)
    {
        Debug.Log("Client " + obj + " Connected");
    }
    protected virtual async void OnClientDisconnected(ulong obj)
    {
        // This runs just before the client is disconnected, client is still technically in the server

        // Host actions
        if (NetworkConnectionController.instance.isServer)
        {
            await CheckClientDisconnect(obj);

            // Runs after the player leaves
            //lobbyData.playerCount = NetworkManager.Singleton.ConnectedClients.Count;
        }

        // Host Disconnect
        if (NetworkManager.ServerClientId == obj)
        {
            LeaveLobby();
        }
    }
    protected async Task CheckClientDisconnect(ulong clientId)
    {
        await Task.Delay(1);
        /*
        if (!NetworkManager.Singleton.ConnectedClientsIds.Contains(clientId))
        {
            return;
        }
        else
        {
            await Task.Delay(1000);
            await CheckClientDisconnect(clientId);
        }
        */
    }

    #endregion

    #region Network Manager Overrides

    public override void OnDestroy()
    {
        // Should never not be this, but just better to check
        if (instance == this)
            instance = null;
    }
    private void OnApplicationQuit()
    {
        LeaveLobby();
    }

    #endregion

}