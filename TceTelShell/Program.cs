using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TceTelShell;

internal static class Program
{
    private const string MutexName = @"Global\TceTelShell.SingleInstance";

    [STAThread]
    static void Main(string[] args)
    {
        using var mutex = new Mutex(true, MutexName, out var createdNew);

        if (!createdNew)
        {
            if (!ActivationPipeClient.TrySend(args))
            {
                MessageBox.Show(
                    "TCE Tel Shell is already running, but the activation message could not be delivered.",
                    "TCE Tel Shell",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            return;
        }

        ApplicationConfiguration.Initialize();
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, ex) =>
            MessageBox.Show(
                $"Unhandled application error.\n\n{ex.Exception.Message}",
                "TCE Tel Shell",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

        var launchArgs = args?.ToArray() ?? Array.Empty<string>();
        var shellForm = new ShellForm();

        _ = Task.Run(() => ActivationPipeServer.Run(shellForm, CancellationToken.None));

        if (launchArgs.Length > 0)
        {
            shellForm.QueueActivation(launchArgs);
        }

        Application.Run(shellForm);
    }
}
