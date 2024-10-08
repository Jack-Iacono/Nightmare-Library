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

    public const int MAX_PLAYERS = 12;
    public const int MIN_PLAYERS = 2;

    public delegate void LobbyEnterDelegate(ulong clientId, bool isServer);
    public static event LobbyEnterDelegate OnLobbyEnter;

    public delegate void UsernameListDelegate();
    public static event UsernameListDelegate OnPlayerListChange;

    public static PlayerList playerList = new PlayerList();

    protected virtual void Awake()
    {
        if (instance != null)
            Destroy(instance);
        instance = this;
    }

    private void Update()
    {
        // Temporary!!!
        // this prevents an error that exists if this line of code is run within the OnClientDisconnected Callback
        if (NetworkConnectionController.HasAuthority)
        {
            UpdatePlayerInfoClientRpc(playerList);
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

        if (NetworkManager.Singleton.IsServer)
        {
            playerList.Add(new PlayerListItem(NetworkManager.LocalClientId, new PlayerInfo(AuthenticationController.playerInfo)));
        }
        else
            AddPlayerInfoServerRpc(NetworkManager.LocalClientId, new PlayerInfo(AuthenticationController.playerInfo));

        OnPlayerListChange?.Invoke();

        return true;
    }

    [ServerRpc(RequireOwnership = false)]
    protected void AddPlayerInfoServerRpc(ulong sender, PlayerInfo playerInfo)
    {
        playerList.Add(new PlayerListItem(sender, playerInfo));
        UpdatePlayerInfoClientRpc(playerList);
    }
    [ClientRpc]
    protected void UpdatePlayerInfoClientRpc(PlayerList pList)
    {
        playerList = pList;
        OnPlayerListChange?.Invoke();
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

    public virtual async void LeaveLobby()
    {
        Debug.Log("Leaving Lobby");
        await DisconnectFromLobby();
        Debug.Log("Disconnected");
        SceneController.LoadScene(SceneController.m_Scene.MAIN_MENU, true);
    }
    public virtual async Task DisconnectFromLobby()
    {
        try
        {
            UnRegisterCallbacks();
        }
        catch
        {
            Debug.Log("Failed to Unregister Callbacks");
        }

        playerList = new PlayerList();

        await NetworkConnectionController.StopConnection();
    }

    #endregion

    #region Callbacks

    protected virtual void RegisterCallbacks()
    {
        Debug.Log("Registering Callbacks");
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

        OnPlayerListChange?.Invoke();
    }
    protected virtual void OnClientDisconnected(ulong obj)
    {
        // This runs just before the client is disconnected, client is still technically in the server

        // Host Disconnect
        if (NetworkConnectionController.HasAuthority && !NetworkManager.ShutdownInProgress)
        {
            playerList.Remove(obj);
            //UpdatePlayerInfoClientRpc(playerList);
        }
        else if(NetworkManager.Singleton != null && !NetworkManager.IsServer && !NetworkManager.ShutdownInProgress)
        {
            Debug.Log(name);
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

        public PlayerList()
        {
            items = new PlayerListItem[0];
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref items);
        }

        public void Add(ulong clientId, PlayerInfo playerInfo)
        {
            Add(new PlayerListItem(clientId, playerInfo));
        }
        public void Add(PlayerListItem newItem)
        {
            List<PlayerListItem> temp = new List<PlayerListItem>();

            for (int i = 0; i < items.Length; i++)
            {
                temp.Add(items[i]);
            }

            temp.Add(newItem);

            items = temp.ToArray();
        }
        public void Remove(ulong clientId)
        {
            List<PlayerListItem> temp = new List<PlayerListItem>();

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].key != clientId)
                    temp.Add(items[i]);
            }

            items = temp.ToArray();
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

        public static PlayerList operator +(PlayerList left, PlayerListItem right)
        {
            left.Add(right);
            return left;
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
                s += items[i].ToString();
            }

            return s;
        }

    }

    #endregion

}