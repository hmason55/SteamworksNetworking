using SteamworksNetworking;
using SteamworksNetworking.Models;
using SteamworksNetworking.Steam;

public class Program
{
    public static async Task Main(string[] args)
    {
        TestPackUnpackMessage();
    }

    public static void TestPackUnpackMessage()
    {
        HelloData data = new()
        {
            Id = "MyId",
            Text = "Hello World"
        };

        Console.WriteLine(data.Print());

        Message input = new((ushort)HelloHandler.MessageType.HelloServer);
        input.Write(data);

        Message input2 = new(input.Buffer.ToArray());

        HelloData outData = input2.Read<HelloData>();

        Console.WriteLine(outData.Print());
    }

    public async Task TestConnect()
    {
        // Replace this with your steam app id.
        uint myAppId = 123;

        // Setup custom handlers.
        SteamNetworkManager.AddHandlers(HelloHandler.Handlers);

        // Connect to Steam
        SteamNetworkManager.Connect(myAppId);

        // Setup a lobby.
        await SteamNetworkManager.Instance.CreateLobby();

    }
}
