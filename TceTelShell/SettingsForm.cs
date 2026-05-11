using System;
using System.Windows.Forms;

namespace TceTelShell;

public class SettingsForm : Form
{
    private readonly Label _titleLabel;
    private readonly CheckBox _embeddedModeCheckBox;
    private readonly Label _urlLabel;
    private readonly TextBox _baseUrlTextBox;
    private readonly Label _pathLabel;
    private readonly TextBox _settingsPathTextBox;
    private readonly Button _saveButton;
    private readonly Button _testButton;
    private readonly Button _defaultAppsButton;

    public SettingsForm()
    {
        Text = "TCE Dialer Settings";
        Width = 700;
        Height = 320;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        _titleLabel = new Label
        {
            Left = 20,
            Top = 20,
            Width = 620,
            Height = 36,
            Text = "Stage 2 uses the embedded desktop shell with the real dialer page loaded in WebView2. Keep the browser URL for fallback or local Next.js development."
        };

        _embeddedModeCheckBox = new CheckBox
        {
            Left = 20,
            Top = 60,
            Width = 320,
            Height = 24,
            Text = "Use embedded desktop dialer shell"
        };

        _urlLabel = new Label
        {
            Left = 20,
            Top = 96,
            Width = 280,
            Height = 20,
            Text = "Embedded dialer / browser fallback URL"
        };

        _baseUrlTextBox = new TextBox
        {
            Left = 20,
            Top = 121,
            Width = 620,
            Height = 24
        };

        _pathLabel = new Label
        {
            Left = 20,
            Top = 156,
            Width = 120,
            Height = 20,
            Text = "Settings file"
        };

        _settingsPathTextBox = new TextBox
        {
            Left = 20,
            Top = 181,
            Width = 620,
            Height = 24,
            ReadOnly = true
        };

        _saveButton = new Button
        {
            Left = 20,
            Top = 225,
            Width = 140,
            Height = 32,
            Text = "Save Settings"
        };

        _testButton = new Button
        {
            Left = 175,
            Top = 225,
            Width = 220,
            Height = 32,
            Text = "Open Browser Fallback URL"
        };

        _defaultAppsButton = new Button
        {
            Left = 410,
            Top = 225,
            Width = 180,
            Height = 32,
            Text = "Open Default Apps"
        };

        _saveButton.Click += SaveSettings;
        _testButton.Click += TestLaunch;
        _defaultAppsButton.Click += OpenDefaultApps;

        Controls.Add(_titleLabel);
        Controls.Add(_embeddedModeCheckBox);
        Controls.Add(_urlLabel);
        Controls.Add(_baseUrlTextBox);
        Controls.Add(_pathLabel);
        Controls.Add(_settingsPathTextBox);
        Controls.Add(_saveButton);
        Controls.Add(_testButton);
        Controls.Add(_defaultAppsButton);

        LoadSettings();
    }

    private void LoadSettings()
    {
        var settings = SettingsManager.Load();
        _embeddedModeCheckBox.Checked = settings.UseEmbeddedDialer;
        _baseUrlTextBox.Text = settings.BaseUrl;
        _settingsPathTextBox.Text = SettingsManager.GetSettingsFilePath();
    }

    private void SaveSettings(object? sender, EventArgs e)
    {
        var baseUrl = _baseUrlTextBox.Text.Trim();

        if (!string.IsNullOrWhiteSpace(baseUrl) &&
            (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) ||
             (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)))
        {
            MessageBox.Show(
                "Please enter a valid http:// or https:// URL for the dialer page.",
                "Invalid URL",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        SettingsManager.Save(new AppSettings
        {
            UseEmbeddedDialer = _embeddedModeCheckBox.Checked,
            BaseUrl = string.IsNullOrWhiteSpace(baseUrl) ? "https://dialer.tcevoiceiq.com/" : baseUrl
        });

        MessageBox.Show(
            "Settings saved. Reload the shell or restart the app for navigation changes to take effect.",
            "TCE Dialer",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void TestLaunch(object? sender, EventArgs e)
    {
        try
        {
            PwaLauncher.OpenBaseUrl();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Unable to open browser fallback",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void OpenDefaultApps(object? sender, EventArgs e)
    {
        DefaultAppsHelper.OpenDefaultApps();
    }
}
