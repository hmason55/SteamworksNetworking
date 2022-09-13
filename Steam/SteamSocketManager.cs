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

    public new void Close()
    {
        try
        {
            if (Connected?.Any() ?? false)
            {
                foreach (Connection connection in Connected ?? new())
                {
                    connection.Flush();
                    connection.Close();
                }

                base.Close();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occured when shutting down the Steam Socket Manager: {e.Message}");
        }
    }
}
