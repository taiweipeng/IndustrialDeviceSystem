[CmdletBinding()]
param(
    [string]$ProfilePath,
    [switch]$Force
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionRoot = Split-Path -Parent $scriptDirectory

function Resolve-StudentProfilePath {
    param(
        [string]$RequestedPath,
        [string]$SearchRoot
    )

    if (-not [string]::IsNullOrWhiteSpace($RequestedPath)) {
        return (Resolve-Path -LiteralPath $RequestedPath).Path
    }

    # The application uses the teacher-selected profile in the solution's
    # config directory. Prefer it when student packages also exist.
    $preferredProfile = Join-Path `
        (Join-Path $SearchRoot "config") `
        "student-profile.json"

    if (Test-Path -LiteralPath $preferredProfile -PathType Leaf) {
        return (Resolve-Path -LiteralPath $preferredProfile).Path
    }

    $profiles = @(
        Get-ChildItem `
            -LiteralPath $SearchRoot `
            -Filter "student-profile.json" `
            -File `
            -Recurse `
            -ErrorAction Stop |
        Where-Object {
            $_.FullName -notmatch `
                "[\\/](bin|obj|generated-student-packages)[\\/]"
        }
    )

    if ($profiles.Count -eq 0) {
        throw "student-profile.json was not found under: $SearchRoot"
    }

    if ($profiles.Count -gt 1) {
        $candidateList =
            ($profiles.FullName | ForEach-Object { "  $_" }) -join `
            [Environment]::NewLine

        throw (
            "Multiple student profiles were found. " +
            "Run this script again with -ProfilePath.`n" +
            $candidateList
        )
    }

    return $profiles[0].FullName
}

$resolvedProfilePath = Resolve-StudentProfilePath `
    -RequestedPath $ProfilePath `
    -SearchRoot $solutionRoot

$profileText = [System.IO.File]::ReadAllText(
    $resolvedProfilePath,
    [System.Text.Encoding]::UTF8)

$profile = $profileText | ConvertFrom-Json
$studentId = [string]$profile.studentId

if ([string]::IsNullOrWhiteSpace($studentId)) {
    throw "studentId is missing from: $resolvedProfilePath"
}

if ($studentId -notmatch "^[A-Za-z0-9_-]{1,64}$") {
    throw "studentId contains invalid path characters: $studentId"
}

$runningApplication = Get-Process `
    -Name "IndustrialDeviceSystem.WinForms" `
    -ErrorAction SilentlyContinue

if ($null -ne $runningApplication) {
    throw "Close IndustrialDeviceSystem.WinForms before resetting the database."
}

$localApplicationData =
    [Environment]::GetFolderPath(
        [Environment+SpecialFolder]::LocalApplicationData)

$databaseDirectory = Join-Path `
    (Join-Path $localApplicationData "IndustrialDeviceSystem") `
    $studentId

$databasePath = Join-Path `
    $databaseDirectory `
    "industrial-device-system.db"

$databaseFiles = @(
    $databasePath,
    "$databasePath-wal",
    "$databasePath-shm"
)

$existingFiles = @(
    $databaseFiles |
    Where-Object { Test-Path -LiteralPath $_ -PathType Leaf }
)

Write-Host "Student ID : $studentId"
Write-Host "Profile    : $resolvedProfilePath"
Write-Host "Database   : $databasePath"

if ($existingFiles.Count -eq 0) {
    Write-Host "No existing database files were found."
    Write-Host "Start the WinForms project to create a clean database."
    return
}

if (-not $Force) {
    Write-Warning "This operation deletes the local database for student $studentId."
    $confirmation = Read-Host "Type RESET to continue"

    if ($confirmation -cne "RESET") {
        Write-Host "Reset cancelled."
        return
    }
}

foreach ($file in $existingFiles) {
    Remove-Item -LiteralPath $file -Force
    Write-Host "Deleted    : $file"
}

Write-Host "Database reset completed."
Write-Host "Start the WinForms project; init.sql, seed.sql and the student profile will be applied automatically."
