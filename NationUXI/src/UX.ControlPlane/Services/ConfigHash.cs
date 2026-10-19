using System.Security.Cryptography;
using System.Text;

namespace UX.ControlPlane.Services;

public static class ConfigHash
{
    public static string ComputeSha256(string json)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
