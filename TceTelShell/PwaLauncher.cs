using System;
using System.Diagnostics;

namespace TceTelShell;

public static class PwaLauncher
{
    public static void OpenDialUrl(string e164Number)
    {
        var settings = SettingsManager.Load();

        if (string.IsNullOrWhiteSpace(settings.BaseUrl))
            throw new InvalidOperationException("Base URL is not configured.");

        var baseUrl = settings.BaseUrl.Trim().TrimEnd('/');

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException("Base URL is invalid. Use http:// or https://");
        }

        var target = $"{baseUrl}/?number={Uri.EscapeDataString(e164Number)}";

        Process.Start(new ProcessStartInfo
        {
            FileName = target,
            UseShellExecute = true
        });
    }

    public static void OpenBaseUrl()
    {
        var settings = SettingsManager.Load();

        if (string.IsNullOrWhiteSpace(settings.BaseUrl))
            throw new InvalidOperationException("Base URL is not configured.");

        var baseUrl = settings.BaseUrl.Trim();

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException("Base URL is invalid. Use http:// or https://");
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = baseUrl,
            UseShellExecute = true
        });
    }
}