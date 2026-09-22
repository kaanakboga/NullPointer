[CmdletBinding()]
param(
    [ValidateSet('Development', 'Release')]
    [string]$Configuration = 'Development',

    [switch]$Clean,

    [switch]$SkipVerification,

    [string]$UnityPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent $PSScriptRoot
$projectVersionFile = Join-Path $projectRoot 'ProjectSettings\ProjectVersion.txt'
$logsDirectory = Join-Path $projectRoot 'Logs\Build'
$buildsRoot = [System.IO.Path]::GetFullPath((Join-Path $projectRoot 'Builds'))
$outputDirectory = [System.IO.Path]::GetFullPath((Join-Path $buildsRoot "Windows-$Configuration"))
$executablePath = Join-Path $outputDirectory 'NullPointer.exe'
$logPath = Join-Path $logsDirectory "windows-$($Configuration.ToLowerInvariant()).log"

function Get-ProjectEditorVersion {
    $versionLine = Select-String -LiteralPath $projectVersionFile -Pattern '^m_EditorVersion:\s*(.+)$'
    if ($null -eq $versionLine) {
        throw "Could not read the Unity editor version from $projectVersionFile."
    }

    return $versionLine.Matches[0].Groups[1].Value.Trim()
}

function Resolve-UnityEditor([string]$requestedPath, [string]$editorVersion) {
    $candidates = [System.Collections.Generic.List[string]]::new()
    if (-not [string]::IsNullOrWhiteSpace($requestedPath)) {
        $candidates.Add($requestedPath)
    }

    if (-not [string]::IsNullOrWhiteSpace($env:UNITY_PATH)) {
        $candidates.Add($env:UNITY_PATH)
    }

    $programFilesRoots = @(
        [Environment]::GetFolderPath([Environment+SpecialFolder]::ProgramFiles),
        [Environment]::GetFolderPath([Environment+SpecialFolder]::ProgramFilesX86)
    ) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Unique

    foreach ($programFilesRoot in $programFilesRoots) {
        $candidates.Add((Join-Path $programFilesRoot "Unity\Hub\Editor\$editorVersion\Editor\Unity.exe"))
        $candidates.Add((Join-Path $programFilesRoot "Unity Hub\Editor\$editorVersion\Editor\Unity.exe"))
    }

    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate -PathType Leaf) {
            return (Resolve-Path -LiteralPath $candidate).Path
        }
    }

    throw "Unity $editorVersion was not found. Pass -UnityPath or set UNITY_PATH to the exact Unity.exe."
}

function Assert-EditorVersion([string]$editorPath, [string]$expectedVersion) {
    $productVersion = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($editorPath).ProductVersion
    if ([string]::IsNullOrWhiteSpace($productVersion) -or -not $productVersion.StartsWith($expectedVersion)) {
        throw "Unity.exe reports '$productVersion'; this project requires '$expectedVersion'."
    }

    Write-Host "Unity editor: $editorPath"
    Write-Host "Unity version: $productVersion"
}

try {
    $editorVersion = Get-ProjectEditorVersion
    $resolvedUnityPath = Resolve-UnityEditor $UnityPath $editorVersion
    Assert-EditorVersion $resolvedUnityPath $editorVersion

    if (-not $outputDirectory.StartsWith($buildsRoot + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to use build output outside the project Builds directory: $outputDirectory"
    }

    if ($Clean -and (Test-Path -LiteralPath $outputDirectory)) {
        Remove-Item -LiteralPath $outputDirectory -Recurse -Force
    }

    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
    New-Item -ItemType Directory -Path $logsDirectory -Force | Out-Null

    if (-not $SkipVerification) {
        $verifyScript = Join-Path $PSScriptRoot 'Verify-Unity.ps1'
        & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $verifyScript -Mode All -UnityPath $resolvedUnityPath
        if ($LASTEXITCODE -ne 0) {
            throw "Unity verification failed before the Windows build."
        }
    }

    $executeMethod = if ($Configuration -eq 'Development') {
        'NullPointer.Editor.WindowsPlayerBuild.BuildDevelopment'
    }
    else {
        'NullPointer.Editor.WindowsPlayerBuild.BuildRelease'
    }

    $unityArguments = @(
        '-batchmode',
        '-nographics',
        '-quit',
        '-projectPath', $projectRoot,
        '-executeMethod', $executeMethod,
        '-buildOutputPath', $executablePath,
        '-logFile', $logPath
    )
    if ($Clean) {
        $unityArguments += '-cleanBuild'
    }

    Write-Host "Building Windows $Configuration player..."
    $escapedArguments = $unityArguments | ForEach-Object {
        if ($_ -match '[\s"]') {
            '"' + $_.Replace('"', '\"') + '"'
        }
        else {
            $_
        }
    }
    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $resolvedUnityPath
    $startInfo.Arguments = $escapedArguments -join ' '
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $unityProcess = [System.Diagnostics.Process]::Start($startInfo)
    $unityProcess.WaitForExit()
    $unityExitCode = $unityProcess.ExitCode
    $unityProcess.Dispose()
    if ($unityExitCode -ne 0) {
        if (Test-Path -LiteralPath $logPath) {
            Get-Content -LiteralPath $logPath -Tail 100
        }

        throw "Unity Windows build failed with exit code $unityExitCode. Inspect $logPath."
    }

    if (-not (Test-Path -LiteralPath $executablePath -PathType Leaf)) {
        throw "Unity reported success but did not create $executablePath."
    }

    $logText = Get-Content -LiteralPath $logPath -Raw
    if ($logText -match 'Build completed with a result of ''Failed''|Aborting batchmode due to failure|Fatal Error!') {
        throw "Unity logged a build failure. Inspect $logPath."
    }

    Write-Host "Windows $Configuration build succeeded."
    Write-Host "Executable: $executablePath"
    Write-Host "Build log: $logPath"
    exit 0
}
catch {
    Write-Error $_
    exit 1
}
