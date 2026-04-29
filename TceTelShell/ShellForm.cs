using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace TceTelShell;

public sealed class ShellForm : Form
{
    private readonly WebView2 _webView;
    private readonly ToolStrip _toolStrip;
    private readonly ToolStripButton _homeButton;
    private readonly ToolStripButton _reloadButton;
    private readonly ToolStripButton _settingsButton;
    private readonly ToolStripLabel _statusLabel;
    private string? _pendingTelUri;
    private bool _webViewReady;
    private bool _dialerReady;

    public ShellForm()
    {
        Text = "TCE VoiceIQ";
        Width = 860;
        Height = 1200;
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new System.Drawing.Size(360, 600);

        _toolStrip = new ToolStrip
        {
            GripStyle = ToolStripGripStyle.Hidden,
            Padding = new Padding(8, 6, 8, 6)
        };

        _homeButton = new ToolStripButton("Home");
        _reloadButton = new ToolStripButton("Reload");
        _settingsButton = new ToolStripButton("Settings");
        _statusLabel = new ToolStripLabel("Initializing shell...")
        {
            Alignment = ToolStripItemAlignment.Right
        };

        _homeButton.Click += async (_, _) => await NavigateHomeAsync();
        _reloadButton.Click += (_, _) => _webView.Reload();
        _settingsButton.Click += (_, _) => OpenSettings();

        _toolStrip.Items.Add(_homeButton);
        _toolStrip.Items.Add(_reloadButton);
        _toolStrip.Items.Add(_settingsButton);
        _toolStrip.Items.Add(_statusLabel);

        _webView = new WebView2
        {
            Dock = DockStyle.Fill,
            DefaultBackgroundColor = System.Drawing.Color.White,
        };

        Controls.Add(_webView);
        Controls.Add(_toolStrip);
        _toolStrip.Dock = DockStyle.Top;

        Shown += async (_, _) => await InitializeWebViewAsync();
    }

    public void QueueActivation(string[] args)
    {
        BringShellToFront();

        if (args.Length == 0)
            return;

        var candidate = args[0];
        if (!candidate.StartsWith("tel:", StringComparison.OrdinalIgnoreCase))
        {
            UpdateStatus($"Received launch argument: {candidate}");
            return;
        }

        var settings = SettingsManager.Load();
        if (!settings.UseEmbeddedDialer)
        {
            try
            {
                var normalizedNumber = TelUriHandler.Normalize(candidate);
                PwaLauncher.OpenDialUrl(normalizedNumber);
                UpdateStatus($"Opened browser fallback for {normalizedNumber}");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Browser fallback failed: {ex.Message}");
            }

            return;
        }

        _pendingTelUri = candidate;
        // Keep the original phrasing but include the actual tel: URI for verification
        UpdateStatus($"Received tel: activation: {candidate}");

        if (_webViewReady)
        {
            _ = DispatchPendingActivationAsync();
        }
    }

    private async System.Threading.Tasks.Task InitializeWebViewAsync()
    {
        if (_webViewReady)
            return;

        try
        {
            var userDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TCE",
                "DialerShell",
                "WebView2");

            Directory.CreateDirectory(userDataFolder);

            var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder);
            await _webView.EnsureCoreWebView2Async(environment);

            var webRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
            _webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "appassets.tce",
                webRoot,
                CoreWebView2HostResourceAccessKind.Allow);

            _webView.CoreWebView2.Settings.AreDevToolsEnabled = true;
            _webView.CoreWebView2.Settings.IsStatusBarEnabled = false;
            _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
            _webView.CoreWebView2.NavigationStarting += (_, e) =>
            {
                _dialerReady = false;
                UpdateStatus("Loading embedded dialer...");
            };

            _webView.CoreWebView2.NavigationCompleted += async (_, e) =>
            {
                if (e.IsSuccess)
                {
                    UpdateStatus("Embedded page loaded. Waiting for dialer ready...");
                    await DispatchPendingActivationAsync();
                }
                else
                {
                    UpdateStatus($"Navigation failed: {e.WebErrorStatus}");
                    NavigateFallbackShell();
                }
            };

            _webView.CoreWebView2.WebMessageReceived += (_, e) => HandleWebMessage(e);

            _webViewReady = true;
            await NavigateHomeAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Unable to initialize the embedded dialer shell.\n\n{ex.Message}",
                "TCE Dialer",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            UpdateStatus("Shell initialization failed.");
        }
    }

    private void HandleWebMessage(CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            using var doc = JsonDocument.Parse(e.WebMessageAsJson);
            var root = doc.RootElement;

            if (!root.TryGetProperty("type", out var typeElement) || typeElement.ValueKind != JsonValueKind.String)
            {
                UpdateStatus("Received message from dialer.");
                return;
            }

            var type = typeElement.GetString() ?? string.Empty;

            switch (type)
            {
                case "dialerReady":
                case "ready":
                    _dialerReady = true;
                    UpdateStatus("Dialer ready.");
                    _ = DispatchPendingActivationAsync();
                    break;

                case "callStatus":
                    if (root.TryGetProperty("status", out var statusElement))
                    {
                        var status = statusElement.GetString() ?? "unknown";
                        UpdateStatus($"Call: {status}");
                    }
                    break;

                case "registrationState":
                    var connected =
                        root.TryGetProperty("connected", out var connectedElement) &&
                        connectedElement.ValueKind == JsonValueKind.True;

                    var registered =
                        root.TryGetProperty("registered", out var registeredElement) &&
                        registeredElement.ValueKind == JsonValueKind.True;

                    UpdateStatus($"SIP {(connected ? "connected" : "disconnected")}, {(registered ? "registered" : "not registered")}");
                    break;

                case "openExternalUrl":
                    if (root.TryGetProperty("url", out var urlElement) && urlElement.ValueKind == JsonValueKind.String)
                    {
                        var url = urlElement.GetString();
                        if (!string.IsNullOrWhiteSpace(url))
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = url,
                                UseShellExecute = true
                            });
                        }
                    }
                    break;

                case "shellLog":
                    if (root.TryGetProperty("message", out var messageElement) && messageElement.ValueKind == JsonValueKind.String)
                    {
                        UpdateStatus(messageElement.GetString() ?? "Shell log received.");
                    }
                    break;

                case "shellWindowState":
                    if (root.TryGetProperty("state", out var stateElement) && stateElement.ValueKind == JsonValueKind.String)
                    {
                        var requestedState = stateElement.GetString() ?? string.Empty;
                        ApplyShellWindowState(requestedState);
                    }
                    break;

                default:
                    UpdateStatus($"Web message: {type}");
                    break;
            }
        }
        catch
        {
            UpdateStatus("Received message from dialer.");
        }
    }
    private async System.Threading.Tasks.Task NavigateHomeAsync()
    {
        if (_webView.CoreWebView2 is null)
            return;

        var settings = SettingsManager.Load();
        var target = ResolveEmbeddedUrl(settings);

        if (target is null)
        {
            NavigateFallbackShell();
            await System.Threading.Tasks.Task.CompletedTask;
            return;
        }

        _webView.CoreWebView2.Navigate(target.ToString());
        await System.Threading.Tasks.Task.CompletedTask;
    }

    private Uri? ResolveEmbeddedUrl(AppSettings settings)
    {
        if (!settings.UseEmbeddedDialer)
            return new Uri("https://appassets.tce/index.html");

        if (!string.IsNullOrWhiteSpace(settings.BaseUrl) &&
            Uri.TryCreate(settings.BaseUrl.Trim(), UriKind.Absolute, out var configuredUri) &&
            (configuredUri.Scheme == Uri.UriSchemeHttp || configuredUri.Scheme == Uri.UriSchemeHttps))
        {
            return configuredUri;
        }

        return new Uri("https://appassets.tce/index.html");
    }

    private void NavigateFallbackShell()
    {
        if (_webView.CoreWebView2 is null)
            return;

        _webView.CoreWebView2.Navigate("https://appassets.tce/index.html");
    }

    private async System.Threading.Tasks.Task DispatchPendingActivationAsync()
    {
        if (!_webViewReady || !_dialerReady || string.IsNullOrWhiteSpace(_pendingTelUri) || _webView.CoreWebView2 is null)
            return;

        try
        {
            var normalizedNumber = TelUriHandler.Normalize(_pendingTelUri);

            var message = new
            {
                type = "dial",
                number = normalizedNumber,
                source = "tel",
                autoDial = false
            };

            _webView.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(message));
            UpdateStatus($"Dial request delivered for {normalizedNumber}");
            _pendingTelUri = null;
        }
        catch (Exception ex)
        {
            UpdateStatus($"Dial activation failed: {ex.Message}");
        }

        await System.Threading.Tasks.Task.CompletedTask;
    }
    private void OpenSettings()
    {
        using var form = new SettingsHubForm();
        form.ShowDialog(this);
    }

    private void BringShellToFront()
    {
        if (WindowState == FormWindowState.Minimized)
        {
            WindowState = FormWindowState.Normal;
        }

        Show();
        Activate();
        TopMost = true;
        TopMost = false;
    }

    private void ApplyShellWindowState(string state)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => ApplyShellWindowState(state)));
            return;
        }

        switch (state.Trim().ToLowerInvariant())
        {
            case "maximized":
            case "maximize":
                WindowState = FormWindowState.Maximized;
                UpdateStatus("Shell view maximized.");
                break;

            case "normal":
            case "normalized":
            case "restore":
                WindowState = FormWindowState.Normal;
                UpdateStatus("Shell view normalized.");
                break;
        }
    }

    private void UpdateStatus(string text)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => UpdateStatus(text)));
            return;
        }

        _statusLabel.Text = text;
    }
}
