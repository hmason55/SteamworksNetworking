using Newtonsoft.Json;
using Steamworks;
using System.Runtime.InteropServices;

namespace SteamworksNetworking;

public static class Extensions
{
    public static byte[] ToBytes(this IntPtr data, int size)
    {
        byte[] bytes = new byte[size];
        Marshal.Copy(data, bytes, 0, size);

        return bytes;
    }

    public static string Print<T>(this T obj) => JsonConvert.SerializeObject(obj, Formatting.Indented);

    public static string AsPossessive(this string name, string yourName) => name == yourName ? $"your" : $"{name}'s";
    public static string AsPossessive(this Friend friend, SteamId yourId) => friend.Id == yourId ? $"your" : $"{friend.Name}'s";
}
