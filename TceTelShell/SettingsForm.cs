using System;
using System.Windows.Forms;

namespace TceTelShell;

public class SettingsForm : Form
{
    private readonly Label _titleLabel;
    private readonly Label _urlLabel;
    private readonly TextBox _baseUrlTextBox;
    private readonly Label _pathLabel;
    private readonly TextBox _settingsPathTextBox;
    private readonly Button _saveButton;
    private readonly Button _testButton;
    private readonly Button _defaultAppsButton;

    public SettingsForm()
    {
        Text = "TCE Tel Shell";
        Width = 640;
        Height = 260;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        _titleLabel = new Label
        {
            Left = 20,
            Top = 20,
            Width = 580,
            Height = 30,
            Text = "Configure the URL that should receive normalized tel: launches."
        };

        _urlLabel = new Label
        {
            Left = 20,
            Top = 60,
            Width = 120,
            Height = 20,
            Text = "Cloud PWA URL"
        };

        _baseUrlTextBox = new TextBox
        {
            Left = 20,
            Top = 85,
            Width = 580,
            Height = 24
        };

        _pathLabel = new Label
        {
            Left = 20,
            Top = 120,
            Width = 120,
            Height = 20,
            Text = "Settings file"
        };

        _settingsPathTextBox = new TextBox
        {
            Left = 20,
            Top = 145,
            Width = 580,
            Height = 24,
            ReadOnly = true
        };

        _saveButton = new Button
        {
            Left = 20,
            Top = 185,
            Width = 140,
            Height = 32,
            Text = "Save Settings"
        };

        _testButton = new Button
        {
            Left = 175,
            Top = 185,
            Width = 160,
            Height = 32,
            Text = "Open Configured URL"
        };

        _defaultAppsButton = new Button
        {
            Left = 350,
            Top = 185,
            Width = 180,
            Height = 32,
            Text = "Open Default Apps"
        };

        _saveButton.Click += SaveSettings;
        _testButton.Click += TestLaunch;
        _defaultAppsButton.Click += OpenDefaultApps;

        Controls.Add(_titleLabel);
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
        _baseUrlTextBox.Text = settings.BaseUrl;
        _settingsPathTextBox.Text = SettingsManager.GetSettingsFilePath();
    }

    private void SaveSettings(object? sender, EventArgs e)
    {
        var baseUrl = _baseUrlTextBox.Text.Trim();

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            MessageBox.Show(
                "Please enter a valid http:// or https:// URL.",
                "Invalid URL",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        SettingsManager.Save(new AppSettings
        {
            BaseUrl = baseUrl
        });

        MessageBox.Show(
            "Settings saved successfully.",
            "TCE Tel Shell",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void TestLaunch(object? sender, EventArgs e)
    {
        try
        {
            SaveSettings(sender, e);
            PwaLauncher.OpenBaseUrl();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Launch Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void OpenDefaultApps(object? sender, EventArgs e)
    {
        try
        {
            DefaultAppsHelper.OpenDefaultApps();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Default Apps Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}