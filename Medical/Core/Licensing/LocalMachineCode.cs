using System.Security.Cryptography;
using System.Text;

namespace Xinglin.Medical.Licensing;

public static class LocalMachineCode
{
    public static string Generate()
    {
        var source = string.Join("|",
            Environment.MachineName,
            Environment.UserDomainName,
            Environment.OSVersion.VersionString,
            Environment.ProcessorCount.ToString());

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(source));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public static bool IsValid(string? code) =>
        code is { Length: 64 } && code.All(Uri.IsHexDigit);
}
