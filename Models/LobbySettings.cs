using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamworksNetworking.Models;

public struct LobbySettings
{
    public LobbySettings(int maxPlayers = 4, bool isPublic = true, bool isJoinable = true, bool isFriendsOnly = false, bool isInvisible = false, params KeyValuePair<string, string>[] data)
    {
        MaxPlayers = maxPlayers;
        IsPublic = isPublic;
        IsJoinable = isJoinable;
        IsFriendsOnly = isFriendsOnly;
        IsInvisible = isInvisible;
        Data = data;
    }

    public int MaxPlayers { get; set; } = 4;
    public bool IsPublic { get; set; } = true;
    public bool IsJoinable { get; set; } = true;
    public bool IsFriendsOnly { get; set; } = false;
    public bool IsInvisible { get; set; } = false;
    public KeyValuePair<string, string>[] Data { get; set; }
}
