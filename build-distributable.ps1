param(
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$appProject = Join-Path $repoRoot 'TceTelShell\TceTelShell.csproj'
$setupProject = Join-Path $repoRoot 'TceTelShell.Setup\TceTelShell.Setup.wixproj'
$publishProfile = 'Distributable'
$msiPath = Join-Path $repoRoot 'TceTelShell.Setup\bin\Release\TceTelShell.Setup.msi'

Write-Host 'Publishing TceTelShell...'
& dotnet publish $appProject -c $Configuration /p:PublishProfile=$publishProfile

Write-Host 'Building WiX installer...'
& dotnet build $setupProject -c $Configuration

if (-not (Test-Path $msiPath)) {
    throw "MSI was not created at expected path: $msiPath"
}

Write-Host ''
Write-Host 'MSI created successfully:'
Write-Host $msiPath
