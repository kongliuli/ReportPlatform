using System.Security.Cryptography;
using System.Text;

namespace ReportDataMaker.Services;

public static class ConfigProtector
{
    private const string EncPrefix = "ENC:";

    public static string Protect(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return plainText;
        var bytes = Encoding.UTF8.GetBytes(plainText);
        var encrypted = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
        return EncPrefix + Convert.ToBase64String(encrypted);
    }

    public static string Unprotect(string value)
    {
        if (string.IsNullOrEmpty(value) || !IsProtected(value)) return value;
        var cipherText = value[EncPrefix.Length..];
        var encrypted = Convert.FromBase64String(cipherText);
        var bytes = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(bytes);
    }

    public static bool IsProtected(string value) => value?.StartsWith(EncPrefix) == true;
}
