using UnityEngine;
using Steamworks;
using Steamworks.Data;
using Netcode.Transports.Facepunch;
using Unity.Netcode;
using System.Linq;


public class GameNetworkManager : MonoBehaviour
{
    // Created for ease of use in other scripts.
    public static GameNetworkManager instance { get; private set; } = null;

    // Variable for the current lobby.Example: When you create a lobby, it is assigned to this change.
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

    // Subscribe to Steam callbacks.
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

    // Unsubscribe from Steam callbacks.
    private void OnDestroy()
    {
        SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite -= OnLobbyInvite;
        SteamMatchmaking.OnLobbyGameCreated -= OnLobbyGameCreated;
        SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;

        // A check is made to see if the NetworkManager is not empty.
        if (NetworkManager.Singleton == null)
            return;

        // Subscribe to Network callbacks.
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
        NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
    }
    #endregion

    #region Steam Callback -> Host Callback 
    // OnLobbyCreated -> The event when you create a lobby.
    private void OnLobbyCreated(Result result, Lobby lobby)
    {
        // Check lobby if not created return
        if(result != Result.OK)
        {
            Debug.Log("Lobby was not created");
            return;
        }

        // Set lobby Settings
        // SetPublic() -> make lobby public.
        // SetJoinable(bool value) -> Allow another player to join the lobby
        // SetData() -> Need for search lobby or set settings IDK :(
        // SetGameServer() -> Allows the owner to set the game server associated with the lobby. Triggers the Steammatchmaking.OnLobbyGameCreated event.
        // SetGameServer() -> Need check for search lobby if work delete SetData()
        lobby.SetPublic();
        lobby.SetJoinable(true);
        lobby.SetData(lobby.Owner.Id.ToString(), "Mark");
        lobby.SetGameServer(lobby.Owner.Id); 

        Debug.Log($"Lobby created {lobby.Owner.Name}");

        // Debug for check lobby.id for connection lobby
        Debug.Log($"Lobby id {lobby.Id}"); 
        // labelLobbyId.text = lobby.Id, In the co-op menu, sets the value of the lobbyID variable for connecting other players. 
        MenuManager.instance.SetLobbyId(lobby.Id.ToString());

        NetworkTransmission.instance.AddMeToDictionaryPlayerServerRPC(SteamClient.SteamId, SteamClient.Name, NetworkManager.Singleton.LocalClientId);
    }

    //OnLobbyEntered -> Event that triggers when you are connected to the lobby.
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
        //Trying connected room 
        RoomEnter joinedLobby = await lobby.Join(); 

        if(joinedLobby != RoomEnter.Success)
        {
            Debug.Log("Failed to create Lobby");
        }
        else
        {
            CurrentLobby = lobby;

            MenuManager.instance.Connected();
            MenuManager.instance.SetLobbyId(lobby.Id.ToString());
            Debug.Log("Joined Lobby");
        }
    }

    private void OnLobbyGameCreated(Lobby lobby, uint ip, ushort port, SteamId id)
    {
        Debug.Log("Lobby was created");
    }
    #endregion

    #region Steam Callback -> Player Callback
    private void OnLobbyMemberJoined(Lobby lobby, Friend steamId)
    {
        Debug.Log($"Join friend {steamId.Name}");
    }
    private void OnLobbyInvite(Friend steamId, Lobby lobby)
    {
        Debug.Log($"Invite friend {steamId.Name}");
    }

    private void OnLobbyMemberLeave(Lobby lobby, Friend steamId)
    {
        Debug.Log($"Leave friend {steamId.Name}");
        NetworkTransmission.instance.RemoveMeToDictionaryPlayerServerRPC(steamId.Id);
    }
    #endregion

    #region Network Function

    // StartHost -> Start the host and subscribe to the callback (OnServerStarted)
    public async void StartHost()
    {
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        NetworkManager.Singleton.StartHost();

        // Adding data about player
        MyInfo.instance.SetValues(NetworkManager.Singleton.LocalClientId, SteamClient.SteamId, SteamClient.Name, true);

        // Creating a lobby with a maximum of 4 players
        CurrentLobby = await SteamMatchmaking.CreateLobbyAsync(4);
    }

    // StartClient -> Start the client and subscribe to the callback (OnClientConnectedCallback & OnClientDisconnectCallback)
    public void StartClient(SteamId id)
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
        // This is for connection, targetID = SteamID.
        transport.targetSteamId = id;

        // Adding data about player
        MyInfo.instance.SetValues(NetworkManager.Singleton.LocalClientId, SteamClient.SteamId, SteamClient.Name, false);

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

        MenuManager.instance.Disconnected();

        Debug.Log("Disconnected");
    }
    #endregion

    #region Network Callback
    private void OnClientConnectedCallback(ulong clientId)
    {
        // Addes in Dictionary Player. Calling ServerRPC
        NetworkTransmission.instance.AddMeToDictionaryPlayerServerRPC(SteamClient.SteamId, SteamClient.Name, clientId);

        //Third Check for host maybe not need -.-
        MyInfo.instance.SetValues(clientId, SteamClient.SteamId, SteamClient.Name, false);

        // Sets the isReady variable in the lobby to false for a new player joined.
        //NetworkTransmission.instance.IsTheClientReadyServerRPC(false, clientId);

        Debug.Log($"Client connected, clientId={clientId}");
    }

    private void OnClientDisconnectCallback(ulong clientId)
    {
        Debug.Log($"Client Disconnect, clientId={clientId}");
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
        if(clientId == 0)
        {
            Disconnect();
        }
    }

    private void OnServerStarted()
    {
        MenuManager.instance.Connected();
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
