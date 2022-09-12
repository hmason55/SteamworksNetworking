using Steamworks;

namespace SteamworksNetworking.Steam;

public class SteamConnectionManager : ConnectionManager
{
    public Action<IntPtr, int> OnConnectionMessage;

    public override void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
    {
        OnConnectionMessage(data, size);
    }
}
