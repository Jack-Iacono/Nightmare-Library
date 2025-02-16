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
        get => NetworkManager.Singleton == null || !NetworkManager.Singleton.IsConnectedClient || NetworkManager.Singleton.IsServer;
    }
    [SerializeField]
    public static bool IsRunning
    {
        get => NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient;
    }

    public static async Task<bool> ConnectToLobby()
    {
        bool connected = false;

        OnProcessActive?.Invoke(true);

        if(!NetworkManager.Singleton.IsConnectedClient && !NetworkManager.Singleton.IsServer)
        {
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

        OnProcessActive?.Invoke(false);

        connectedToLobby = connected;
        return connected;
    }
    public static async Task StartConnection()
    {
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

    private static async Task CheckConnectionStart()
    {
        if(NetworkManager.Singleton.IsConnectedClient)
        {
            OnProcessActive?.Invoke(false);
            OnConnected?.Invoke();

            currentConnectionTimer = 0;
            Debug.Log("Connection Successful");

            return;
        }
        else
        {
            OnProcessActive?.Invoke(false);

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

    public static async Task StopConnection()
    {
        instance.StopAllCoroutines();

        await VoiceChatController.LeaveChannel();
        await StopLobby();
        StopNetworkManager();

        allocation = null;
        connectedToLobby = false;
    }
    public static void StopNetworkManager()
    {
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

    private static async Task<bool> CreateLobby()
    {
        try
        {
            if(allocation == null)
                allocation = await RelayService.Instance.CreateAllocationAsync(MAX_PLAYERS);
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            SetTransformAsHost(allocation);

            OnJoinCodeChanged?.Invoke(instance, joinCode);

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
    private static async Task<bool> JoinLobby()
    {
        try
        {
            var a = await RelayService.Instance.JoinAllocationAsync(joinCode);

            // Set the details to the transform
            SetTransformAsClient(a);

            Debug.Log("Joined Lobby: " + joinCode);

            return true;
        }
        catch (Exception e)
        {
            Debug.Log("Lobby Not Found: " + e);
            return false;
        }
    }

    protected static void SetTransformAsClient(JoinAllocation a)
    {
        transport.SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData);
    }
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

        transport = FindObjectOfType<UnityTransport>();

        DontDestroyOnLoad(this);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            transport.SetHostRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, new byte[64], allocation.ConnectionData);
        }
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
