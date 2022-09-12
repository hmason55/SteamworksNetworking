using Newtonsoft.Json;
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
}
