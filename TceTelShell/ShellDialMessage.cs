namespace TceTelShell;

internal sealed class ShellDialMessage
{
    public string Type { get; set; } = "dial";
    public string Number { get; set; } = string.Empty;
    public string Source { get; set; } = "tel";
    public bool AutoDial { get; set; } = false;
}
