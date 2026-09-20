[CmdletBinding()]
param(
    [ValidateSet('All', 'Compile', 'EditMode', 'PlayMode')]
    [string]$Mode = 'All',

    [string]$UnityPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent $PSScriptRoot
$projectVersionFile = Join-Path $projectRoot 'ProjectSettings\ProjectVersion.txt'
$logsDirectory = Join-Path $projectRoot 'Logs\Verification'

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

function Assert-UnityLog([string]$logPath) {
    if (-not (Test-Path -LiteralPath $logPath -PathType Leaf)) {
        throw "Unity did not produce the expected log: $logPath"
    }

    $logText = Get-Content -LiteralPath $logPath -Raw
    $failurePatterns = @(
        'error CS\d{4}:',
        'Scripts have compiler errors',
        'Compilation failed',
        'Aborting batchmode due to failure',
        'Fatal Error!'
    )

    foreach ($pattern in $failurePatterns) {
        if ($logText -match $pattern) {
            throw "Unity reported a compile or batch failure. Inspect $logPath."
        }
    }
}

function Invoke-UnityStep([string]$stepName, [string[]]$stepArguments) {
    $logPath = Join-Path $logsDirectory "$stepName.log"
    $arguments = @(
        '-batchmode',
        '-nographics',
        '-projectPath', $projectRoot,
        '-logFile', $logPath
    ) + $stepArguments

    Write-Host "Running Unity step '$stepName'..."
    $escapedArguments = $arguments | ForEach-Object {
        if ($_ -match '[\s"]') {
            '"' + $_.Replace('"', '\"') + '"'
        }
        else {
            $_
        }
    }

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $script:resolvedUnityPath
    $startInfo.Arguments = $escapedArguments -join ' '
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true

    $process = [System.Diagnostics.Process]::Start($startInfo)
    $process.WaitForExit()
    $exitCode = $process.ExitCode
    $process.Dispose()

    if ($exitCode -ne 0) {
        if (Test-Path -LiteralPath $logPath) {
            Write-Host (Get-Content -LiteralPath $logPath -Tail 80 | Out-String)
        }

        throw "Unity step '$stepName' failed with exit code $exitCode. Inspect $logPath."
    }

    Assert-UnityLog $logPath
    Write-Host "Unity step '$stepName' passed. Log: $logPath"
}

function Assert-TestResults([string]$resultPath, [string]$platformName) {
    if (-not (Test-Path -LiteralPath $resultPath -PathType Leaf)) {
        throw "$platformName tests did not produce results at $resultPath."
    }

    [xml]$results = Get-Content -LiteralPath $resultPath -Raw
    $testRun = $results.'test-run'
    if ($null -eq $testRun) {
        throw "Could not parse $platformName NUnit results at $resultPath."
    }

    $total = [int]$testRun.total
    $failed = [int]$testRun.failed
    $passed = [int]$testRun.passed

    if ($total -le 0) {
        throw "$platformName test run discovered no tests."
    }

    if ($failed -gt 0) {
        throw "$platformName tests failed: $failed failed out of $total. Inspect $resultPath."
    }

    Write-Host "$platformName tests passed: $passed/$total. Results: $resultPath"
}

try {
    $editorVersion = Get-ProjectEditorVersion
    $script:resolvedUnityPath = Resolve-UnityEditor $UnityPath $editorVersion
    Assert-EditorVersion $script:resolvedUnityPath $editorVersion

    New-Item -ItemType Directory -Path $logsDirectory -Force | Out-Null

    $projectLock = Join-Path $projectRoot 'Temp\UnityLockfile'
    if (Test-Path -LiteralPath $projectLock) {
        Write-Warning 'A Unity lock file exists. Verification will fail safely if another editor owns this project.'
    }

    if ($Mode -in @('All', 'Compile')) {
        Invoke-UnityStep 'compile' @('-quit')
    }

    if ($Mode -in @('All', 'EditMode')) {
        $editModeResults = Join-Path $logsDirectory 'editmode-results.xml'
        Invoke-UnityStep 'editmode-tests' @(
            '-runTests',
            '-testPlatform', 'EditMode',
            '-testResults', $editModeResults
        )
        Assert-TestResults $editModeResults 'EditMode'
    }

    if ($Mode -in @('All', 'PlayMode')) {
        $playModeResults = Join-Path $logsDirectory 'playmode-results.xml'
        Invoke-UnityStep 'playmode-tests' @(
            '-runTests',
            '-testPlatform', 'PlayMode',
            '-testResults', $playModeResults
        )
        Assert-TestResults $playModeResults 'PlayMode'
    }

    Write-Host "Unity verification completed successfully for mode '$Mode'."
    exit 0
}
catch {
    Write-Error $_
    exit 1
}
