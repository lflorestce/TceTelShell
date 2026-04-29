using System;

namespace TceTelShell;

public class AccountSettings
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberCredentials { get; set; } = false;
    public bool AutoFillLogin { get; set; } = true;
    public bool AutoSubmitLogin { get; set; } = false;

    public bool HasUsableCredentials =>
        !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
}
