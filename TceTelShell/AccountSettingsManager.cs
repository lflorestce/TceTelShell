using System;
using System.IO;
using System.Text.Json;

namespace TceTelShell;

public static class AccountSettingsManager
{
    private static readonly string SettingsFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "TCE",
        "TelShell");

    private static readonly string SettingsFile = Path.Combine(SettingsFolder, "account-settings.json");

    private sealed class PersistedAccountSettings
    {
        public string Email { get; set; } = string.Empty;
        public string ProtectedPassword { get; set; } = string.Empty;
        public bool RememberCredentials { get; set; } = false;
        public bool AutoFillLogin { get; set; } = true;
        public bool AutoSubmitLogin { get; set; } = false;
    }

    public static AccountSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsFile))
                return new AccountSettings();

            var json = File.ReadAllText(SettingsFile);
            var persisted = JsonSerializer.Deserialize<PersistedAccountSettings>(json) ?? new PersistedAccountSettings();

            return new AccountSettings
            {
                Email = persisted.Email,
                Password = string.IsNullOrWhiteSpace(persisted.ProtectedPassword)
                    ? string.Empty
                    : SecureSecretStore.Unprotect(persisted.ProtectedPassword),
                RememberCredentials = persisted.RememberCredentials,
                AutoFillLogin = persisted.AutoFillLogin,
                AutoSubmitLogin = persisted.AutoSubmitLogin
            };
        }
        catch
        {
            return new AccountSettings();
        }
    }

    public static void Save(AccountSettings settings)
    {
        Directory.CreateDirectory(SettingsFolder);

        var persisted = new PersistedAccountSettings
        {
            Email = settings.Email?.Trim() ?? string.Empty,
            ProtectedPassword = settings.RememberCredentials && !string.IsNullOrWhiteSpace(settings.Password)
                ? SecureSecretStore.Protect(settings.Password)
                : string.Empty,
            RememberCredentials = settings.RememberCredentials,
            AutoFillLogin = settings.AutoFillLogin,
            AutoSubmitLogin = settings.AutoSubmitLogin && settings.RememberCredentials
        };

        var json = JsonSerializer.Serialize(persisted, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(SettingsFile, json);
    }

    public static void Clear()
    {
        try
        {
            if (File.Exists(SettingsFile))
                File.Delete(SettingsFile);
        }
        catch
        {
            // Intentionally ignored. Caller can reload and verify.
        }
    }

    public static string GetSettingsFilePath() => SettingsFile;
}
