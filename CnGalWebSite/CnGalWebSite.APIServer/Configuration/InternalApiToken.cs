using System.Security.Cryptography;
using System.Text;

namespace CnGalWebSite.APIServer.Configuration;

public static class InternalApiToken
{
    public static bool Matches(string configured, string supplied)
    {
        return !string.IsNullOrWhiteSpace(configured) &&
            !string.IsNullOrWhiteSpace(supplied) &&
            CryptographicOperations.FixedTimeEquals(
                SHA256.HashData(Encoding.UTF8.GetBytes(configured)),
                SHA256.HashData(Encoding.UTF8.GetBytes(supplied)));
    }
}
