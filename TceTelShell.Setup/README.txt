Installer UX update for TCE Tel Shell

Included:
- Updated Package.wxs with:
  - branded installer images
  - Add/Remove Programs icon
  - Start Menu shortcut
  - optional 'Launch TCE Tel Shell' checkbox on the final dialog
- Updated TceTelShell.Setup.wixproj with WiX UI and Util extension package references
- Assets folder with:
  - TceTelShell.ico
  - WixUIBanner.png
  - WixUIDialog.png

Recommended next step on your Windows build machine:
1. Copy Assets into TceTelShell.Setup\Assets
2. Replace Package.wxs and TceTelShell.Setup.wixproj with the updated versions
3. Delete bin and obj under TceTelShell.Setup
4. Restore/build the solution again

If Visual Studio/NuGet prompts for restore, allow it.
