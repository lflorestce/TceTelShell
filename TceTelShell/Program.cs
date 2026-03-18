using System;
using System.Windows.Forms;

namespace TceTelShell;

internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        if (args.Length > 0 && args[0].StartsWith("tel:", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                TelUriHandler.Handle(args[0]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to process tel link.\n\n{ex.Message}",
                    "TCE Tel Shell",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            return;
        }

        Application.Run(new SettingsForm());
    }
}