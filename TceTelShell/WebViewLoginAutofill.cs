using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;

namespace TceTelShell;

public static class WebViewLoginAutofill
{
    public static bool ShouldHandleLoginPage(Uri? uri)
    {
        if (uri is null)
            return false;

        return uri.AbsolutePath.Equals("/auth/login", StringComparison.OrdinalIgnoreCase);
    }

    public static async Task TryInjectSavedCredentialsAsync(CoreWebView2 webView)
    {
        var account = AccountSettingsManager.Load();

        if (!account.RememberCredentials || !account.AutoFillLogin || !account.HasUsableCredentials)
            return;

        var payload = JsonSerializer.Serialize(new
        {
            email = account.Email,
            password = account.Password,
            autoSubmit = account.AutoSubmitLogin
        });

        var script = $$"""
            (function() {
                const data = {{payload}};
                const email = document.querySelector('input[name="Email"]');
                const password = document.querySelector('input[name="Password"]');
                if (!email || !password) {
                    return JSON.stringify({ success: false, reason: 'login-inputs-not-found' });
                }

                email.focus();
                email.value = data.email || '';
                email.dispatchEvent(new Event('input', { bubbles: true }));
                email.dispatchEvent(new Event('change', { bubbles: true }));

                password.focus();
                password.value = data.password || '';
                password.dispatchEvent(new Event('input', { bubbles: true }));
                password.dispatchEvent(new Event('change', { bubbles: true }));

                const form = email.closest('form') || password.closest('form');
                if (data.autoSubmit && form) {
                    form.requestSubmit();
                }

                return JSON.stringify({ success: true, autoSubmit: !!data.autoSubmit });
            })();
            """;

        await webView.ExecuteScriptAsync(script);
    }
}
