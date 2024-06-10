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
    public static LobbyController Instance { get; protected set; }
    public static List<MonoBehaviour> observers = new List<MonoBehaviour>();

    public const int MAX_PLAYERS = 12;
    public const int MIN_PLAYERS = 2;

    protected async virtual void Start()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;

        // Attempts to join a lobby, if that doesn't work, leave
        if (!NetworkConnectionController.connectedToLobby)
        {
            if (await NetworkConnectionController.ConnectToLobby())
                await NetworkConnectionController.StartConnection();
            else
                Disconnect();
        }

        // Both the server-host and client(s) register the custom named message.
        RegisterCallbacks();

        if (NetworkManager.Singleton.IsServer)
            ServerEntryAction();
        else
            ClientEntryAction();
    }
    protected virtual void ServerEntryAction()
    {
        Debug.Log("Run Server Entry");
    }
    protected virtual void ClientEntryAction()
    {
        Debug.Log("Run Client Entry");
    }

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
            Disconnect();
        }
    }
    protected async Task CheckClientDisconnect(ulong clientId)
    {
        if (!NetworkManager.Singleton.ConnectedClientsIds.Contains(clientId))
        {
            return;
        }
        else
        {
            await Task.Delay(1000);
            await CheckClientDisconnect(clientId);
        }
    }

    #endregion

    #region Lobby Moving

    /*public virtual async void TransferLobby(SceneController.Scene scene)
    {
        await NetworkConnectionController.StopLobby();

        // Sends the lobby transfer message to everyone
        if (NetworkConnectionController.instance.isServer)
        {
            SendLobbyMessage(LobbyTransferMessage, new LobbyTransferData(scene));
        }
    }*/
    public virtual void TransferLobby(int index)
    {
        StartCoroutine(delay(index));
    }

    protected virtual async void Disconnect()
    {
        try
        {
            UnRegisterCallbacks();
        }
        catch
        {
            Debug.Log("Failed to Unregister Callbacks");
        }

        await NetworkConnectionController.StopConnection();

        //SceneController.LoadScene(SceneController.Scene.MAIN_MENU);
    }
    private IEnumerator delay(int index)
    {
        yield return new WaitForSeconds(1.5f);
        //TransferLobby((SceneController.Scene)index);
    }

    #endregion

    #region Network Manager Overrides

    public override void OnDestroy()
    {
        // Should never not be this, but just better to check
        if (Instance == this)
            Instance = null;
    }
    private void OnApplicationQuit()
    {
        Disconnect();
    }

    #endregion
}