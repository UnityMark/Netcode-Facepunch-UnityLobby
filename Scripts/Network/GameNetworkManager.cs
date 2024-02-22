using UnityEngine;
using Steamworks;
using Steamworks.Data;
using Netcode.Transports.Facepunch;
using Unity.Netcode;
using System.Linq;

public class GameNetworkManager : MonoBehaviour
{
    public static GameNetworkManager instance { get; private set; } = null;
    public Lobby? CurrentLobby { get; private set; } = null;
    private FacepunchTransport transport = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    #region Life Cycle Unity
    private void Start()
    {
        transport = GetComponent<FacepunchTransport>();

        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;
        SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreated;


        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
    }

    private void OnDestroy()
    {
        SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite -= OnLobbyInvite;
        SteamMatchmaking.OnLobbyGameCreated -= OnLobbyGameCreated;
        SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;

        if (NetworkManager.Singleton == null)
            return;

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
        NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
    } 
    #endregion

    #region Steam Callback -> Host Callback 
    private void OnLobbyCreated(Result result, Lobby lobby)
    {
        if(result != Result.OK)
        {
            Debug.Log("Lobby was not created");
            return;
        }
        lobby.SetPublic();
        lobby.SetJoinable(true);
        lobby.SetData(lobby.Owner.Id.ToString(), "Mark");
        lobby.SetGameServer(lobby.Owner.Id);

        Debug.Log($"Lobby created {lobby.Owner.Name}");
        Debug.Log($"Lobby id {lobby.Id}"); // for connection
        MenuManager.instance.SetLobbyId(lobby.Id.ToString());
    }

    private void OnLobbyEntered(Lobby lobby)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            return;
        }

        StartClient(CurrentLobby.Value.Owner.Id);
    }
    #endregion

    #region Steam Callback -> Lobby Callback

    //When you accept invite or join on a friend
    private async void OnGameLobbyJoinRequested(Lobby lobby, SteamId id)
    {
        RoomEnter joinedLobby = await lobby.Join();

        if(joinedLobby != RoomEnter.Success)
        {
            Debug.Log("Failed to create Lobby");
        }
        else
        {
            CurrentLobby = lobby;
            Debug.Log("Joined Lobby");
        }
    }

    private void OnLobbyGameCreated(Lobby lobby, uint ip, ushort port, SteamId id)
    {
        Debug.Log("Lobby was created");
    }
    #endregion

    #region Steam Callback -> Player Callback
    private void OnLobbyMemberJoined(Lobby lobby, Friend friend)
    {
        Debug.Log($"Join friend {friend.Name}");
    }
    private void OnLobbyInvite(Friend friend, Lobby lobby)
    {
        Debug.Log($"Invite friend {friend.Name}");
    }

    private void OnLobbyMemberLeave(Lobby lobby, Friend friend)
    {
        Debug.Log($"Leave friend {friend.Name}");
    }
    #endregion

    #region Network Function
    public async void StartHost()
    {
        //First
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        NetworkManager.Singleton.StartHost();

        //Second
        MyInfo.instance.SetValues(NetworkManager.Singleton.LocalClientId, SteamClient.SteamId, SteamClient.Name);

        //First
        CurrentLobby = await SteamMatchmaking.CreateLobbyAsync(4);
    }

    public void StartClient(SteamId id)
    {
        //First
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
        transport.targetSteamId = id;

        //Second
        MyInfo.instance.SetValues(NetworkManager.Singleton.LocalClientId, SteamClient.SteamId, SteamClient.Name);

        //First
        if (NetworkManager.Singleton.StartClient())
        {
            Debug.Log("Client has joined", this);
        }
    }

    public void Disconnect()
    {
        CurrentLobby?.Leave();

        if (NetworkManager.Singleton == null)
        {
            return;
        }

        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        }
        else
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
        }

        NetworkManager.Singleton.Shutdown(true);

        //Second
        MenuManager.instance.Disconnected();

        //First
        Debug.Log("Disconnected");
    }
    #endregion

    #region Network Callback
    private void OnClientConnectedCallback(ulong clientId)
    {
        Debug.Log($"Client connected, clientId={clientId}");
    }

    private void OnClientDisconnectCallback(ulong clientId)
    {
        Debug.Log($"Client Disconnect, clientId={clientId}");
    }

    private void OnServerStarted()
    {
        Debug.Log("Host started", this);
    }
    #endregion

    #region Join From Id !!!WARING TEST!!!
    [ContextMenu("GetLobby")]
    public async void GoToLobby()
    {
        Lobby[] lobbies = await SteamMatchmaking.LobbyList.RequestAsync();
        foreach (Lobby lobby in lobbies.ToList())
        {
            if (lobby.Id.ToString() == MenuManager.instance.GetInputField())
            {
                RoomEnter joinedLobbySuccess = await lobby.Join();
                if (joinedLobbySuccess != RoomEnter.Success)
                {
                    Debug.Log("failed to join lobby");
                }
                else
                {
                    CurrentLobby = lobby;
                }
                
            }
        }

        MenuManager.instance.SetLobbyId(CurrentLobby.Value.Id.ToString());
    }

    public async void GoToLobbyData()
    {
        Lobby[] lobbies = await SteamMatchmaking.LobbyList.WithKeyValue(MenuManager.instance.GetInputField(), "Mark").RequestAsync();
        foreach (Lobby lobby in lobbies.ToList())
        {

        }
    } 
    #endregion

    private void OnApplicationQuit()
    {
        Disconnect();
    }
}
