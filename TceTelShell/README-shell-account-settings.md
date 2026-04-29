TCE Dialer Shell - Settings Split + Secure Account Storage

This patch adds a clean way to split shell settings into:
- App Settings
- Account Settings

It also adds secure storage for the dialer login credentials by using Windows DPAPI through `ProtectedData`, scoped to the current Windows user profile.

Files included
- AccountSettings.cs
- SecureSecretStore.cs
- AccountSettingsManager.cs
- AccountSettingsForm.cs
- SettingsHubForm.cs
- WebViewLoginAutofill.cs

Recommended wiring steps in the current shell project

1) Add these files to the existing `TceTelShell` project.

2) Change any current shell Settings entry point from:
   `new SettingsForm().ShowDialog(this);`
   to:
   `new SettingsHubForm().ShowDialog(this);`

3) In the WebView2 host form, after navigation completes, add logic similar to:

   if (WebViewLoginAutofill.ShouldHandleLoginPage(new Uri(webView.Source)))
   {
       await WebViewLoginAutofill.TryInjectSavedCredentialsAsync(webView.CoreWebView2);
   }

   If your host uses a navigation-completed event, that is the simplest place to do it.

4) Keep App Settings unchanged. `SettingsHubForm` opens the existing `SettingsForm` for that.

5) Account settings are stored at:
   %LOCALAPPDATA%\TCE\TelShell\account-settings.json

Behavior
- Credentials are only stored if the user enables "Remember credentials on this PC".
- Password is not stored in plain text.
- Auto-fill can populate the embedded login page.
- Auto-submit is optional and should stay off unless you really want a near-silent sign-in flow.

Notes
- This design intentionally keeps the shell thin. The shell owns secure local storage and Windows behavior; the embedded app still owns the login page and auth flow.
- If you later want logout from the shell to clear embedded browser data as well, that should be handled through the WebView2 profile/user-data-folder cleanup path rather than only deleting the account settings file.
