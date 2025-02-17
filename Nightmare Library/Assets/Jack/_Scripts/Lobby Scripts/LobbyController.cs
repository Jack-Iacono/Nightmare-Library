using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;
using Unity.VisualScripting;
using System.Linq;
using System.Threading.Tasks;

public abstract class LobbyController : NetworkBehaviour
{
    public static LobbyController instance { get; protected set; }
    public static List<MonoBehaviour> observers = new List<MonoBehaviour>();

    public const int MAX_PLAYERS = 4;
    public const int MIN_PLAYERS = 1;

    public delegate void LobbyEnterDelegate(ulong clientId, bool isServer);
    public static event LobbyEnterDelegate OnLobbyEnter;

    public delegate void UsernameListDelegate();
    public static event UsernameListDelegate OnPlayerListChange;

    public static NetworkVariable<PlayerList> playerList = new NetworkVariable<PlayerList>();

    protected virtual void Awake()
    {
        if (instance != null)
            Destroy(instance);

        instance = this;
    }

    protected virtual void Start()
    {
        if (NetworkConnectionController.connectedToLobby)
        {
            RegisterCallbacks();

            if (NetworkManager.Singleton.IsServer)
                ServerEntryAction();
            else
                ClientEntryAction();
        }
        else
        {
            EntryAction();
        }
    }

    #region Entry Methods

    public async static Task<bool> StartConnection()
    {
        // Attempts to join a lobby, if that doesn't work, leave
        if (!NetworkConnectionController.connectedToLobby)
        {
            instance.RegisterCallbacks();

            if (await NetworkConnectionController.ConnectToLobby())
                await NetworkConnectionController.StartConnection();
            else
            {
                instance.LeaveLobby();
                return false;
            }
        }
        
        if(NetworkManager.Singleton.IsServer)
            playerList.Value = new PlayerList();

        if (NetworkManager.Singleton.IsServer)
        {
            playerList.Value = new PlayerList(new PlayerListItem(NetworkManager.Singleton.LocalClientId, new PlayerInfo(AuthenticationController.playerInfo)));
        }
        else
        {
            instance.AddPlayerInfoServerRpc(NetworkManager.Singleton.LocalClientId, new PlayerInfo(AuthenticationController.playerInfo));
        }

        return true;
    }

    [ServerRpc(RequireOwnership = false)]
    protected void AddPlayerInfoServerRpc(ulong sender, PlayerInfo playerInfo)
    {
        playerList.Value = new PlayerList(playerList.Value, new PlayerListItem(sender, playerInfo));
    }
    protected void OnPlayerInfoChanged(PlayerList previous, PlayerList current)
    {
        OnPlayerListChange?.Invoke();
    }

    protected virtual void ServerEntryAction()
    {
        OnLobbyEnter?.Invoke(NetworkManager.LocalClientId, true);
        ConnectVoiceChat();
    }
    protected virtual void ClientEntryAction()
    {
        OnLobbyEnter?.Invoke(NetworkManager.LocalClientId, false);
        ConnectVoiceChat();
    }

    protected virtual void ConnectVoiceChat()
    {

    }

    /// <summary>
    /// Called when offline player enters the lobby
    /// </summary>
    protected virtual void EntryAction() { }

    #endregion

    #region Lobby Leaving

    public virtual async void LeaveLobby()
    {
        //Debug.Log("Leaving Lobby");
        await DisconnectFromLobby();
        //Debug.Log("Disconnected");
        SceneController.LoadScene(SceneController.m_Scene.MAIN_MENU, true);
    }
    public virtual async Task DisconnectFromLobby()
    {
        try
        {
            UnRegisterCallbacks();
        }
        catch(Exception ex)
        {
            Debug.Log("Failed to Unregister Callbacks with Exception: " + ex);
        }

        await NetworkConnectionController.StopConnection();
    }

    #endregion

    #region Callbacks

    protected virtual void RegisterCallbacks()
    {
        NetworkManager.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;

        playerList.OnValueChanged += OnPlayerInfoChanged;
    }
    protected virtual void UnRegisterCallbacks()
    {
        if(NetworkManager.Singleton != null)
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;

            playerList.OnValueChanged -= OnPlayerInfoChanged;
        }
    }
    
    protected virtual void OnClientConnected(ulong obj)
    {
        Debug.Log("Client " + obj + " Connected");
    }
    protected virtual void OnClientDisconnected(ulong obj)
    {
        // This runs just before the client is disconnected, client is still technically in the server

        // Host Disconnect
        if (NetworkConnectionController.HasAuthority && !NetworkManager.ShutdownInProgress)
        {
            playerList.Value = new PlayerList(playerList.Value, obj);
            OnPlayerListChange?.Invoke();
            Debug.Log(playerList.Value.ToString());
        }
        else if(NetworkManager.Singleton != null && !NetworkManager.IsServer && !NetworkManager.ShutdownInProgress)
        {
            LeaveLobby();
        }
    }
    protected async Task CheckClientDisconnect(ulong clientId)
    {
        await Task.Delay(1);
        
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

    #region Network Manager Overrides

    public override void OnDestroy()
    {
        // Should never not be this, but just better to check
        if (instance == this)
            instance = null;

        UnRegisterCallbacks();
    }
    private void OnApplicationQuit()
    {
        LeaveLobby();
    }

    #endregion

    #region Player List Class Defenitions

    public struct PlayerInfo : INetworkSerializable
    {
        public string username;
        public string id;

        public PlayerInfo(Unity.Services.Authentication.PlayerInfo info)
        {
            if(info.Username != null)
                username = info.Username;
            else
                username = "Null User " + info.Id.ToString();

            id = info.Id;
        }

        public override string ToString()
        {
            return username + " " + id;
        }

        public static bool operator ==(PlayerInfo left, PlayerInfo right)
        {
            return left.id == right.id && left.username == right.username;
        }
        public static bool operator !=(PlayerInfo left, PlayerInfo right)
        {
            return left.id != right.id || left.username != right.username;
        }

        public override bool Equals(object obj)
        {
            return this == (PlayerInfo)obj;
        }
        public override int GetHashCode()
        {
            int hash = 0;
            hash ^= username.GetHashCode();
            hash ^= id.GetHashCode();
            return hash;
        }

        public class EqualityComparer : IEqualityComparer<PlayerInfo>
        {
            public bool Equals(PlayerInfo x, PlayerInfo y)
            {
                if (x.username == null && y.username == null)
                    return x.username == y.username;
                return x.username == y.username && x.id == y.id;
            }
            public int GetHashCode(PlayerInfo obj)
            {
                int hash = 0;
                if (obj.username != null)
                    hash ^= obj.username.GetHashCode();
                hash ^= obj.id.GetHashCode();
                return hash;
            }
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref username);
            serializer.SerializeValue(ref id);
        }
    }
    public struct PlayerListItem : INetworkSerializable
    {
        public ulong key;
        public PlayerInfo value;

        public PlayerListItem(ulong key, PlayerInfo value)
        {
            this.key = key;
            this.value = value;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref value);
            serializer.SerializeValue(ref key);
        }

        public override string ToString()
        {
            return key.ToString() + ": " + value.ToString();
        }
    }
    public class PlayerList : INetworkSerializable
    {
        private PlayerListItem[] items;

        public int Count
        {
            get => items.Length;
        }
        public List<ulong> Keys
        {
            get
            {
                List<ulong> result = new List<ulong>();
                for(int i = 0; i < items.Length; i++)
                {
                    result.Add(items[i].key);
                }
                return result;
            }
        }

        // All of these function as add, remove, etc since network variables don't update normally otherwise
        public PlayerList()
        {
            items = new PlayerListItem[0];
        }
        public PlayerList(PlayerListItem n)
        {
            items = new PlayerListItem[] { n };
        }
        // Used for adding user to PlayerList
        public PlayerList(PlayerList previous, PlayerListItem n)
        {
            items = new PlayerListItem[previous.Count + 1];
            for(int i = 0; i < previous.Count; i++)
            {
                items[i] = previous.items[i];
            }
            items[items.Length - 1] = n;
        }
        // Used for removing user from PlayerList
        public PlayerList(PlayerList previous, ulong id)
        {
            if(previous.ContainsKey(id))
                items = new PlayerListItem[previous.Count - 1];
            else
                items = new PlayerListItem[previous.Count];

            int index = 0;
            for(int i = 0; i < items.Length; i++)
            {
                if (previous.items[i].key != id)
                {
                    items[index] = previous.items[i];
                    index++;
                }
            }
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref items);
        }

        public bool ContainsKey(ulong clientId)
        {
            for(int i = 0; i < items.Length; i++)
            {
                if (items[i].key == clientId)
                    return true;
            }
            return false;
        }
        public bool ContainsValue(PlayerInfo playerInfo)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].value == playerInfo)
                    return true;
            }
            return false;
        }

        public Dictionary<ulong, PlayerInfo> GetDictionary()
        {
            Dictionary<ulong, PlayerInfo> dict = new Dictionary<ulong, PlayerInfo>();
            for(int i = 0; i < items.Length; i++)
            {
                PlayerListItem n = items[i];
                dict.Add(n.key, n.value);
            }
            return dict;
        }
        public PlayerInfo GetPlayerInfo(ulong id)
        {
            for(int i = 0; i < items.Length; i++)
            {
                if (items[i].key == id)
                    return items[i].value;
            }
            return items[0].value;
        }
        public override string ToString()
        {
            string s = String.Empty;

            for(int i = 0; i < items.Length; i++)
            {
                s += items[i].ToString() + "\n";
            }

            return s;
        }

    }

    #endregion

}