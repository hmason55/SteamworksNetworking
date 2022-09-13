using SteamworksNetworking.Models;
using SteamworksNetworking.Steam;

public class HelloHandler
{
    public enum MessageType : ushort
    {
        HelloServer = 0,
        HelloClient = 1
    }

    public static event Action<HelloData> OnHello;

    public readonly static Dictionary<ushort, MessageHandler> Handlers = new()
    {
        { (ushort)MessageType.HelloServer, HelloServer },
        { (ushort)MessageType.HelloClient, HelloClient }
    };

    public static void HelloServer(ulong senderId, Message message)
    {
        // Only the server should receive this data.
        if (!SteamNetworkManager.IsHost)
        {
            return;
        }

        Console.WriteLine($"Received a 'HelloServer' packet.");
        HelloData value = message.Read<HelloData>();

        OnHello(value);
    }

    public static void HelloClient(ulong senderId, Message message)
    {
        // Only clients should receive this data.
        if (SteamNetworkManager.IsHost)
        {
            return;
        }

        Console.WriteLine($"Received a 'HelloClient' packet.");
        HelloData value = message.Read<HelloData>();

        OnHello(value);
    }
}
