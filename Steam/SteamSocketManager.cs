using Steamworks;
using Steamworks.Data;

namespace SteamworksNetworking.Steam;

public class SteamSocketManager : SocketManager
{
    public event Action<Connection, IntPtr, int> OnSocketMessage;

    public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
    {
        OnSocketMessage(connection, data, size);
    }
}
