using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;
using Unity.VisualScripting;
using Unity.Netcode;

// Used to get the Max Players
using static LobbyController;
using Unity.Services.Core;
using Unity.Networking.Transport.Relay;

public class NetworkConnectionController : NetworkBehaviour
{
    private static List<MonoBehaviour> observers = new List<MonoBehaviour>();

    public static NetworkConnectionController instance { get; private set; }

    public static Allocation allocation = null;

    public static string joinCode { get; set; }
    public static bool connectedToLobby = false;

    protected static UnityTransport transport;

    public const string PublicLobbyJoinKey = "PublicLobbyJoinKey";
    public static Lobby connectedLobby;

    public enum ConnectionType { CREATE, JOIN };
    public static ConnectionType connectionType { get; set; }

    private const int ConnectionTimeoutTimer = ConnectionRetryTimer * 5;
    private const int ConnectionRetryTimer = 1000;
    private static float currentConnectionTimer = 0;

    public static event EventHandler<string> OnJoinCodeChanged;

    public delegate void ProcessActiveDelegate(bool inProgress);
    public static event ProcessActiveDelegate OnProcessActive;

    public delegate void ProcessCompleteDelegate();
    public static event ProcessCompleteDelegate OnConnected;

    [SerializeField]
    public static bool HasAuthority
    {
        get => !connectedToLobby || (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer);
    }
    [SerializeField]
    public static bool IsRunning
    {
        get => NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient;
    }

    public static bool CheckNetworkConnected(NetworkBehaviour networkBehaviour)
    {
        if (!connectedToLobby)
        {
            Destroy(networkBehaviour);
            Destroy(networkBehaviour.GetComponent<NetworkObject>());

            return false;
        }

        return true;
    }

    /// <summary>
    /// Connects the player to a lobby based on the connection type that they have previously set
    /// </summary>
    /// <returns></returns>
    public static async Task<bool> ConnectToLobby()
    {
        bool connected = false;

        // Tell the game that a connection process is active
        OnProcessActive?.Invoke(true);

        // Initialize the UnityServices (i.e. Relay and Netcode)
        await UnityServices.InitializeAsync();

        // Check to see if the player is not already connected to the server in any capacity
        if(!NetworkManager.Singleton.IsConnectedClient && !NetworkManager.Singleton.IsServer)
        {
            // Either create or join a lobby based on the player's connection type selected previously
            switch (connectionType)
            {
                case ConnectionType.CREATE:
                    connected = await CreateLobby();
                    break;
                case ConnectionType.JOIN:
                    connected = await JoinLobby();
                    break;
                default:
                    connected = false;
                    break;
            }
        }

        // Tell other scripts that the connection process is finished
        OnProcessActive?.Invoke(false);

        connectedToLobby = connected;
        return connected;
    }

    /// <summary>
    /// Begins the Network Manager as either a Client or Host depending on the connectionType variable
    /// </summary>
    /// <returns></returns>
    public static async Task StartNetworkManager()
    {
        // Check to make sure the user is not already connected 
        if (!NetworkManager.Singleton.IsConnectedClient && !NetworkManager.Singleton.IsHost)
        {
            switch (connectionType)
            {
                case ConnectionType.CREATE:
                    NetworkManager.Singleton.StartHost();
                    break;
                case ConnectionType.JOIN:
                    NetworkManager.Singleton.StartClient();
                    break;
            }
            
            await CheckConnectionStart();
        }
    }

    /// <summary>
    /// Checks the connection to the lobby and retrys if necessary
    /// </summary>
    /// <returns></returns>
    private static async Task CheckConnectionStart()
    {
        // If the player is connected, this script is done
        if(NetworkManager.Singleton.IsConnectedClient)
        {
            OnProcessActive?.Invoke(false);
            OnConnected?.Invoke();

            currentConnectionTimer = 0;
            Debug.Log("Connection Successful");

            return;
        }
        // If the player is not connected yet
        else
        {
            OnProcessActive?.Invoke(false);

            // Check if the max number of connection attempts has been made and, if so, end the connection attempts
            if (currentConnectionTimer <= ConnectionTimeoutTimer)
            {
                Debug.Log("Connection Not Started, Trying Again in 1 Second");
                await Task.Delay(ConnectionRetryTimer);
                currentConnectionTimer += ConnectionRetryTimer;
                await CheckConnectionStart();
            }
            else
            {
                Debug.Log("Connection Timed Out");
                return;
            }
        }

        currentConnectionTimer = 0;
    }

    /// <summary>
    /// Stops all connection the network
    /// </summary>
    /// <returns></returns>
    public static async Task StopConnection()
    {
        // Stop the background processes
        instance.StopAllCoroutines();

        // Disconnect the voice chat, lobby and the network manager
        await VoiceChatController.LeaveChannel();
        await StopLobby();
        StopNetworkManager();

        // Clear all previously selected options from connection
        allocation = null;
        connectedToLobby = false;
    }
    public static void StopNetworkManager()
    {
        // What do you think it does genius?
        try
        {
            NetworkManager.Singleton.Shutdown();
        }
        catch (Exception ex)
        {
            Debug.Log("Network Shutdown Error: " + ex);
        }
    }
    public static async Task StopLobby()
    {
        instance.StopAllCoroutines();

        //Disconnects the client/host
        if (connectedLobby != null)
        {
            try
            {
                // Disconnects from the lobby, and if host shut it down
                if (instance.IsHost)
                {
                    await LobbyService.Instance.DeleteLobbyAsync(connectedLobby.Id);
                }
                else
                {
                    await LobbyService.Instance.RemovePlayerAsync(connectedLobby.Id, AuthenticationController.playerInfo.Id);
                }
            }
            catch
            {
                
            }

            connectedLobby = null;
        }
    }

    /// <summary>
    /// Creates a new relay session with this machine as the host
    /// </summary>
    /// <returns></returns>
    private static async Task<bool> CreateLobby()
    {
        try
        {
            // If the allocation has not yet been set, get a new allocation from the Relay Service
            if(allocation == null)
                allocation = await RelayService.Instance.CreateAllocationAsync(MAX_PLAYERS);
            // Get the join code for the current allocation
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // Set this allocation as the host transform for the room
            SetTransformAsHost(allocation);

            // Alert other scripts that the join code has been changed
            OnJoinCodeChanged?.Invoke(instance, joinCode);

            // Automatically copy the join code to the clipboard
            TextEditor te = new TextEditor();
            te.text = NetworkConnectionController.joinCode;
            te.SelectAll();
            te.Copy();

            return true;
        }
        catch (Exception e)
        {
            Debug.LogFormat("Failed Creating a Lobby: " + e);
            return false;
        }
    }
    /// <summary>
    /// Joins a lobby with the code from joinCode
    /// </summary>
    /// <returns></returns>
    private static async Task<bool> JoinLobby()
    {
        try
        {
            // Get the allocation related to the given join code
            var a = await RelayService.Instance.JoinAllocationAsync(joinCode);

            // Set this machine as a client on the allocation
            SetTransformAsClient(a);

            return true;
        }
        catch (Exception e)
        {
            Debug.Log("Lobby Not Found: " + e);
            return false;
        }
    }

    /// <summary>
    /// Sends the necessary allocation data from the Client to the Relay Server for binding
    /// </summary>
    /// <param name="a">The allocation that is being used</param>
    protected static void SetTransformAsClient(JoinAllocation a)
    {
        transport.SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData);
    }
    /// <summary>
    /// Sends the necessary allocation data from the Host to the Relay Server for binding
    /// </summary>
    /// <param name="a">The allocation that is being used</param>
    protected static void SetTransformAsHost(Allocation a)
    {
        transport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);
    }

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    private void Start()
    {
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
    }

    private async void OnApplicationQuit()
    {
        await StopConnection();
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
    }
}
