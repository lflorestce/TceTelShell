using System;
using System.Security.Cryptography;
using System.Text;

namespace TceTelShell;

public static class SecureSecretStore
{
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("TCE Dialer Desktop Account Settings v1");

    public static string Protect(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        var raw = Encoding.UTF8.GetBytes(plainText);
        var protectedBytes = ProtectedData.Protect(raw, Entropy, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(protectedBytes);
    }

    public static string Unprotect(string protectedValue)
    {
        if (string.IsNullOrWhiteSpace(protectedValue))
            return string.Empty;

        var protectedBytes = Convert.FromBase64String(protectedValue);
        var raw = ProtectedData.Unprotect(protectedBytes, Entropy, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(raw);
    }
}
