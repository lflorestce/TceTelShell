# TCE Tel Shell distributable prep

This bundle updates the solution so you can generate a cleaner distributable MSI.

## What changed

- `TceTelShell.csproj`
  - Ensures `wwwroot` is copied to the publish output.
- `TceTelShell/Properties/PublishProfiles/Distributable.pubxml`
  - Publishes `win-x64`, `Release`, `self-contained`, `single-file`.
  - Leaves `wwwroot` outside the single executable so the shell can still load the local fallback page.
- `TceTelShell.Setup/Package.wxs`
  - Adds `MajorUpgrade` support.
  - Embeds the cabinet into the MSI.
  - Installs the published EXE plus `wwwroot/index.html`.
  - Adds a Start Menu shortcut.
- `build-distributable.ps1`
  - Runs publish, then builds the WiX project.

## Build steps

From the solution root in PowerShell:

```powershell
.\build-distributable.ps1
```

Expected MSI output:

```text
TceTelShell.Setup\bin\Release\TceTelShell.Setup.msi
```

## Notes

- This keeps WebView2 on the Evergreen model; the runtime itself is not bundled in the MSI.
- Test on a clean Windows machine or VM before broad distribution.
