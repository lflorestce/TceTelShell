using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;

namespace TceTelShell;

internal static class ActivationPipeClient
{
    private const string PipeName = "TceTelShell.Activation";

    public static bool TrySend(string[] args)
    {
        try
        {
            using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
            client.Connect(1500);

            using var writer = new StreamWriter(client, Encoding.UTF8, leaveOpen: true)
            {
                AutoFlush = true
            };

            var payload = string.Join('\n', args?.Select(a => a ?? string.Empty) ?? Array.Empty<string>());
            writer.Write(payload);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
