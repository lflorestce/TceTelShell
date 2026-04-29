using System;

namespace TceTelShell;

public static class TelUriHandler
{
    public static void Handle(string telUri)
    {
        if (!telUri.StartsWith("tel:", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Input is not a tel: URI.");

        var e164 = Normalize(telUri);
        PwaLauncher.OpenDialUrl(e164);
    }

    public static string Normalize(string telUri)
    {
        if (!telUri.StartsWith("tel:", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Input is not a tel: URI.");

        var raw = telUri.Substring(4);
        var numberPart = raw.Split(';')[0];
        return PhoneNormalizer.ToE164(numberPart);
    }
}
