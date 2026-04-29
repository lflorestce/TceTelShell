using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TceTelShell;

internal static class ActivationPipeServer
{
    private const string PipeName = "TceTelShell.Activation";

    public static async Task Run(ShellForm shellForm, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && !shellForm.IsDisposed)
        {
            try
            {
                using var server = new NamedPipeServerStream(
                    PipeName,
                    PipeDirection.In,
                    1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                await server.WaitForConnectionAsync(cancellationToken);

                using var reader = new StreamReader(server, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
                var payload = await reader.ReadToEndAsync(cancellationToken);

                if (string.IsNullOrWhiteSpace(payload))
                    continue;

                var args = payload
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                if (args.Length == 0)
                    continue;

                shellForm.BeginInvoke(new Action(() => shellForm.QueueActivation(args)));
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch
            {
                await Task.Delay(500, cancellationToken);
            }
        }
    }
}
