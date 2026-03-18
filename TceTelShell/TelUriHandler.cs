using System;

namespace TceTelShell;

public static class TelUriHandler
{
    public static void Handle(string telUri)
    {
        if (!telUri.StartsWith("tel:", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Input is not a tel: URI.");

        var raw = telUri.Substring(4);

        // Strip URI parameters such as ;ext=123 if present
        var numberPart = raw.Split(';')[0];

        var e164 = PhoneNormalizer.ToE164(numberPart);

        PwaLauncher.OpenDialUrl(e164);
    }
}