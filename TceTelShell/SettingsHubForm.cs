using System;
using System.Windows.Forms;

namespace TceTelShell;

public class SettingsHubForm : Form
{
    private readonly Label _titleLabel;
    private readonly Label _bodyLabel;
    private readonly Button _appSettingsButton;
    private readonly Button _accountSettingsButton;
    private readonly Button _closeButton;

    public SettingsHubForm()
    {
        Text = "TCE Dialer Settings";
        Width = 460;
        Height = 260;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        _titleLabel = new Label
        {
            Left = 20,
            Top = 20,
            Width = 400,
            Height = 26,
            Text = "Choose the settings area you want to manage."
        };

        _bodyLabel = new Label
        {
            Left = 20,
            Top = 52,
            Width = 400,
            Height = 38,
            Text = "App Settings controls the shell and fallback dialer URL. Account Settings stores the embedded dialer login details securely for this Windows user profile."
        };

        _appSettingsButton = new Button
        {
            Left = 20,
            Top = 105,
            Width = 180,
            Height = 40,
            Text = "App Settings"
        };

        _accountSettingsButton = new Button
        {
            Left = 220,
            Top = 105,
            Width = 180,
            Height = 40,
            Text = "Account Settings"
        };

        _closeButton = new Button
        {
            Left = 160,
            Top = 155,
            Width = 120,
            Height = 32,
            Text = "Close"
        };

        _appSettingsButton.Click += (_, _) =>
        {
            using var form = new SettingsForm();
            form.ShowDialog(this);
        };

        _accountSettingsButton.Click += (_, _) =>
        {
            using var form = new AccountSettingsForm();
            form.ShowDialog(this);
        };

        _closeButton.Click += (_, _) => Close();

        Controls.Add(_titleLabel);
        Controls.Add(_bodyLabel);
        Controls.Add(_appSettingsButton);
        Controls.Add(_accountSettingsButton);
        Controls.Add(_closeButton);
    }
}
