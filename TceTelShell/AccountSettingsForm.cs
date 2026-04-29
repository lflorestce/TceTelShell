using System;
using System.Windows.Forms;

namespace TceTelShell;

public class AccountSettingsForm : Form
{
    private readonly Label _titleLabel;
    private readonly Label _emailLabel;
    private readonly TextBox _emailTextBox;
    private readonly Label _passwordLabel;
    private readonly TextBox _passwordTextBox;
    private readonly CheckBox _rememberCheckBox;
    private readonly CheckBox _autoFillCheckBox;
    private readonly CheckBox _autoSubmitCheckBox;
    private readonly Label _pathLabel;
    private readonly TextBox _pathTextBox;
    private readonly Button _saveButton;
    private readonly Button _clearButton;
    private readonly Button _closeButton;

    public AccountSettingsForm()
    {
        Text = "TCE Dialer Account Settings";
        Width = 640;
        Height = 440;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        _titleLabel = new Label
        {
            Left = 20,
            Top = 20,
            Width = 580,
            Height = 36,
            Text = "Store the dialer account credentials securely on this Windows profile so the embedded login page can be prefilled when needed."
        };

        _emailLabel = new Label
        {
            Left = 20,
            Top = 70,
            Width = 120,
            Height = 20,
            Text = "Account email"
        };

        _emailTextBox = new TextBox
        {
            Left = 20,
            Top = 95,
            Width = 580,
            Height = 24
        };

        _passwordLabel = new Label
        {
            Left = 20,
            Top = 130,
            Width = 120,
            Height = 20,
            Text = "Password"
        };

        _passwordTextBox = new TextBox
        {
            Left = 20,
            Top = 155,
            Width = 580,
            Height = 24,
            UseSystemPasswordChar = true
        };

        _rememberCheckBox = new CheckBox
        {
            Left = 20,
            Top = 190,
            Width = 260,
            Height = 24,
            Text = "Remember credentials on this PC"
        };

        _autoFillCheckBox = new CheckBox
        {
            Left = 20,
            Top = 215,
            Width = 260,
            Height = 24,
            Text = "Auto-fill login page"
        };

        _autoSubmitCheckBox = new CheckBox
        {
            Left = 20,
            Top = 240,
            Width = 260,
            Height = 24,
            Text = "Auto-submit login form"
        };

        _pathLabel = new Label
        {
            Left = 20,
            Top = 270,
            Width = 120,
            Height = 20,
            Text = "Settings file"
        };

        _pathTextBox = new TextBox
        {
            Left = 20,
            Top = 295,
            Width = 580,
            Height = 24,
            ReadOnly = true
        };

        _saveButton = new Button
        {
            Left = 20,
            Top = 325,
            Width = 140,
            Height = 32,
            Text = "Save Settings"
        };

        _clearButton = new Button
        {
            Left = 175,
            Top = 325,
            Width = 160,
            Height = 32,
            Text = "Clear Saved Credentials"
        };

        _closeButton = new Button
        {
            Left = 350,
            Top = 325,
            Width = 120,
            Height = 32,
            Text = "Close"
        };

        _saveButton.Click += SaveSettings;
        _clearButton.Click += ClearSavedCredentials;
        _closeButton.Click += (_, _) => Close();
        _rememberCheckBox.CheckedChanged += (_, _) => SyncState();
        _autoFillCheckBox.CheckedChanged += (_, _) => SyncState();

        Controls.Add(_titleLabel);
        Controls.Add(_emailLabel);
        Controls.Add(_emailTextBox);
        Controls.Add(_passwordLabel);
        Controls.Add(_passwordTextBox);
        Controls.Add(_rememberCheckBox);
        Controls.Add(_autoFillCheckBox);
        Controls.Add(_autoSubmitCheckBox);
        Controls.Add(_pathLabel);
        Controls.Add(_pathTextBox);
        Controls.Add(_saveButton);
        Controls.Add(_clearButton);
        Controls.Add(_closeButton);

        LoadSettings();
        SyncState();
    }

    private void LoadSettings()
    {
        var settings = AccountSettingsManager.Load();
        _emailTextBox.Text = settings.Email;
        _passwordTextBox.Text = settings.Password;
        _rememberCheckBox.Checked = settings.RememberCredentials;
        _autoFillCheckBox.Checked = settings.AutoFillLogin;
        _autoSubmitCheckBox.Checked = settings.AutoSubmitLogin;
        _pathTextBox.Text = AccountSettingsManager.GetSettingsFilePath();
    }

    private void SyncState()
    {
        _autoFillCheckBox.Enabled = _rememberCheckBox.Checked;
        _autoSubmitCheckBox.Enabled = _rememberCheckBox.Checked && _autoFillCheckBox.Checked;

        if (!_rememberCheckBox.Checked)
        {
            _autoFillCheckBox.Checked = false;
            _autoSubmitCheckBox.Checked = false;
        }
        else if (!_autoFillCheckBox.Checked)
        {
            _autoSubmitCheckBox.Checked = false;
        }
    }

    private void SaveSettings(object? sender, EventArgs e)
    {
        var email = _emailTextBox.Text.Trim();
        var password = _passwordTextBox.Text;

        if (_rememberCheckBox.Checked)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show(
                    "Please enter the account email before saving.",
                    "Missing Email",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show(
                    "Please enter the account password before saving.",
                    "Missing Password",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
        }

        AccountSettingsManager.Save(new AccountSettings
        {
            Email = email,
            Password = password,
            RememberCredentials = _rememberCheckBox.Checked,
            AutoFillLogin = _autoFillCheckBox.Checked,
            AutoSubmitLogin = _autoSubmitCheckBox.Checked
        });

        MessageBox.Show(
            "Account settings saved successfully.",
            "TCE Dialer",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void ClearSavedCredentials(object? sender, EventArgs e)
    {
        AccountSettingsManager.Clear();
        _emailTextBox.Text = string.Empty;
        _passwordTextBox.Text = string.Empty;
        _rememberCheckBox.Checked = false;
        _autoFillCheckBox.Checked = false;
        _autoSubmitCheckBox.Checked = false;
        _pathTextBox.Text = AccountSettingsManager.GetSettingsFilePath();
        SyncState();

        MessageBox.Show(
            "Saved credentials were cleared.",
            "TCE Dialer",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }
}
