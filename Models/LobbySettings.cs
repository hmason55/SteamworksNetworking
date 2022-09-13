namespace SteamworksNetworking.Models;

public struct LobbySettings
{
    public LobbySettings(
        int maxPlayers = DEFAULT_MAX_PLAYERS, 
        bool isPublic = DEFAULT_IS_PUBLIC, 
        bool isJoinable = DEFAULT_IS_JOINABLE, 
        bool isFriendsOnly = DEFAULT_IS_FRIENDS_ONLY, 
        bool isInvisible = DEFAULT_IS_INVISIBLE, 
        params KeyValuePair<string, string>[] data)
    {
        _maxPlayers = maxPlayers;
        _isPublic = isPublic;
        _isJoinable = isJoinable;
        _isFriendsOnly = isFriendsOnly;
        _isInvisible = isInvisible;
        Data = data;
    }

    private const int DEFAULT_MAX_PLAYERS = 4;
    private const bool DEFAULT_IS_PUBLIC = true;
    private const bool DEFAULT_IS_JOINABLE = true;
    private const bool DEFAULT_IS_FRIENDS_ONLY = false;
    private const bool DEFAULT_IS_INVISIBLE = false;

    private int? _maxPlayers;
    private bool? _isPublic;
    private bool? _isJoinable;
    private bool? _isFriendsOnly;
    private bool? _isInvisible;

    public int MaxPlayers { get => _maxPlayers ?? DEFAULT_MAX_PLAYERS; set => _maxPlayers = value; }
    public bool IsPublic { get => _isPublic ?? DEFAULT_IS_PUBLIC; set => _isPublic = value; }
    public bool IsJoinable { get => _isJoinable ?? DEFAULT_IS_JOINABLE; set => _isJoinable = value; }
    public bool IsFriendsOnly { get => _isFriendsOnly ?? DEFAULT_IS_FRIENDS_ONLY; set => _isFriendsOnly = value; }
    public bool IsInvisible { get => _isInvisible ?? DEFAULT_IS_INVISIBLE; set => _isInvisible = value; }
    public KeyValuePair<string, string>[] Data { get; set; }




}
