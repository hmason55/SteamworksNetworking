using Steamworks;
using Steamworks.Data;
using SteamworksNetworking.Models;

namespace SteamworksNetworking.Steam;

public sealed class SteamNetworkManager : IDisposable
{
    private static readonly Lazy<SteamNetworkManager> lazy = new(() => new());
    public static SteamNetworkManager Instance { get { return lazy.Value; } }
    private SteamNetworkManager() { }

    private static SteamConnectionManager ConnectionManager { get; set; }
    private static SteamSocketManager SocketManager { get; set; }

    public static uint AppId { get => SteamClient.AppId; }
    public static Lobby CurrentLobby { get; private set; }
    public static bool IsSteamConnected { get; private set; }
    public static bool IsHost => CurrentLobby.IsOwnedBy(SteamClient.SteamId);

    private static Dictionary<ushort, MessageHandler> Handlers = new();

    public static event Action<Lobby, uint, ushort, SteamId> OnLobbyGameCreated;
    public static event Action<Result, Lobby> OnLobbyCreated;
    public static event Action<Lobby> OnLobbyEntered;
    public static event Action<Lobby, Friend> OnLobbyMemberJoined;
    public static event Action<Lobby, Friend, string> OnChatMessage;
    public static event Action<Lobby, Friend> OnLobbyMemberDisconnected;
    public static event Action<Lobby, Friend> OnLobbyMemberLeave;
    public static event Action<Lobby, SteamId> OnGameLobbyJoinRequested;

    private static CancellationTokenSource ReceiveDataTokenSource = new();
    private bool disposedValue;

    private static Task ReceiveDataTask { get; set; }

    public static void AddHandlers(IEnumerable<KeyValuePair<ushort, MessageHandler>> handlers)
    {
        foreach(KeyValuePair<ushort, MessageHandler> handler in handlers)
        {
            if(!Handlers.TryAdd(handler.Key, handler.Value))
            {
                Console.WriteLine($"Failed to add handler with key {handler.Key} because it already exists in the collection.");
            }
        }
    }

    private static void SetupCallbacks()
    {
        SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreated;
        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyCreated += CreateSocketServer;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyEntered += JoinSocketServer;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnChatMessage += OnChatMessage;
        SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberDisconnected;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
    }

    public static void Connect(uint appId)
    {
        if(IsSteamConnected)
        {
            return;
        }

        try
        {
            SetupCallbacks();

            // Create client
            SteamClient.Init(appId, true);

            if (!SteamClient.IsValid)
            {
                Console.WriteLine("Invalid Steam client.");
                throw new Exception();
            }

            IsSteamConnected = true;

            ReceiveDataTask ??= Task.Run(() => ReceiveData());

            Console.WriteLine($"Connected to Steam as {SteamClient.Name}.");
        }
        catch (Exception e)
        {
            IsSteamConnected = false;
            Console.WriteLine($"An error occurred while connecting to Steam: {e.Message}");
        }
    }

    public static void Disconnect()
    {
        LeaveLobby();
        SteamClient.Shutdown();
        IsSteamConnected = false;

        Console.WriteLine($"You disconnected from Steam.");
    }

    public static void RunCallbacks()
    {
        SteamClient.RunCallbacks();

        if(SocketManager != null)
        {
            SocketManager.Receive();
        }

        if (ConnectionManager != null)
        {
            ConnectionManager.Receive();
        }
    }

    public static void BroadcastMessage(byte[] data)
    {
        if (!IsHost)
        {
            return;
        }

        // Relay the message to other clients.
        foreach (Connection connection in SocketManager.Connected.Skip(1))
        {
            connection.SendMessage(data);
        }
    }

    public static void SendMessage(Connection connection, byte[] data)
    {
        if (!IsHost)
        {
            return;
        }

        connection.SendMessage(data);
    }

    public static void SendMessage(byte[] data)
    {
        if (IsHost)
        {
            return;
        }

        ConnectionManager.Connection.SendMessage(data);
    }

    private static void OnSocketMessage(Connection connection, IntPtr data, int size) => HandleMessage(connection.Id, new(data.ToBytes(size)));
    private static void OnConnectionMessage(IntPtr data, int size) => HandleMessage(CurrentLobby.Owner.Id, new(data.ToBytes(size)));
    
    public async Task<bool> CreateLobby(LobbySettings settings = default)
    {
        try
        {
            Console.WriteLine("Creating lobby...");
            Lobby? createdLobby = await SteamMatchmaking.CreateLobbyAsync(settings.MaxPlayers);

            if (!createdLobby.HasValue)
            {
                throw new Exception("Unable to create lobby.");
            }

            Lobby lobby = createdLobby.Value;

            if(settings.IsPublic)
            {
                lobby.SetPublic();
            }
            else
            {
                lobby.SetPrivate();
            }

            lobby.SetJoinable(settings.IsJoinable);

            if(settings.IsFriendsOnly)
            {
                lobby.SetFriendsOnly();
            }

            if(settings.IsInvisible)
            {
                lobby.SetInvisible();
            }

            if(settings.Data != null)
            {
                foreach (KeyValuePair<string, string> kvp in settings.Data)
                {
                    lobby.SetData(kvp.Key, kvp.Value);
                }
            }

            CurrentLobby = lobby;

            Console.WriteLine("You created a lobby.");
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to create lobby: {e.Message}");
            return false;
        }
    }

    private static void CreateSocketServer(Result result, Lobby lobby)
    {
        if (!IsHost)
        {
            return;
        }

        SocketManager = SteamNetworkingSockets.CreateRelaySocket<SteamSocketManager>(0);
        SocketManager.OnSocketMessage += OnSocketMessage;

        ConnectionManager = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(SteamClient.SteamId);
        ConnectionManager.OnConnectionMessage += OnConnectionMessage;

        Console.WriteLine($"You created a socket server.");
    }

    private static void JoinSocketServer(Lobby lobby)
    {
        if (IsHost)
        {
            return;
        }

        ConnectionManager = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(lobby.Owner.Id, 0);
        ConnectionManager.OnConnectionMessage += OnConnectionMessage;

        Console.WriteLine($"Joined {lobby.Owner.AsPossessive(SteamClient.SteamId)} socket server.");
    }

    private static void HandleMessage(ulong senderId, Message message)
    {
        ushort id = message.ReadUShort();

        try
        {
            if (Handlers.TryGetValue(id, out MessageHandler handler))
            {
                handler.Invoke(senderId, message);
            }
            else
            {
                Console.WriteLine($"Received unknown packet with id: {id}");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occurred when handling the message: {e.Message}");
        }
    }

    public static void LeaveLobby()
    {
        CurrentLobby.Leave();
        Console.WriteLine($"You left {CurrentLobby.Owner.AsPossessive(SteamClient.SteamId)} lobby.");

        ReceiveDataTokenSource.Cancel();
        ConnectionManager?.Close();
        SocketManager?.Close();
    }

    private static void ReceiveData()
    {
        while (true)
        {
            if(ReceiveDataTokenSource.Token.IsCancellationRequested)
            {
                ReceiveDataTask = null;
                return;
            }

            if (IsSteamConnected)
            {
                RunCallbacks();
            }
        }
    }

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                return;
            }

            Disconnect();
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}