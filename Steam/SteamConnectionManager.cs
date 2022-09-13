using Steamworks;

namespace SteamworksNetworking.Steam;

public class SteamConnectionManager : ConnectionManager
{
    public Action<IntPtr, int> OnConnectionMessage;

    public override void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
    {
        OnConnectionMessage(data, size);
    }

    public new void Close()
    {
        try
        {
            if (Connected)
            {
                Connection.Flush();
                Connection.Close();
                base.Close();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occured when shutting down the Steam Connection Manager: {e.Message}");
        }
    }
}
