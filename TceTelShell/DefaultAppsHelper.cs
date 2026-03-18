using System.Diagnostics;

namespace TceTelShell;

public static class DefaultAppsHelper
{
    public static void OpenDefaultApps()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "ms-settings:defaultapps",
            UseShellExecute = true
        });
    }
}