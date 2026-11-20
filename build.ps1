# build.ps1 - Cross-platform build script for Sento Launcher (Windows PowerShell)
# Run on each target platform or use GitHub Actions

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet('win', 'mac', 'linux', 'all')]
    [string]$Target = 'all',
    
    [string]$Config = 'Release',
    [string]$Project = 'SentoLauncher.csproj',
    [string]$OutputDir = 'publish'
)

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Sento Launcher - Cross-Platform Build" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

function Build-RID {
    param(
        [string]$RID,
        [string]$OutputName
    )
    
    Write-Host ""
    Write-Host "Building for $RID..." -ForegroundColor Yellow
    Write-Host "-----------------------------------------"
    
    $args = @(
        'publish', $Project,
        '-c', $Config,
        '-r', $RID,
        '--self-contained', 'true',
        '-p:PublishSingleFile=true',
        '-p:PublishTrimmed=true',
        '-p:TrimMode=partial',
        '-o', "$OutputDir/$OutputName",
        '--no-restore'
    )
    
    $exitCode = (Start-Process 'dotnet' -ArgumentList $args -Wait -PassThru).ExitCode
    
    if ($exitCode -eq 0) {
        Write-Host "✓ $RID build successful: $OutputDir/$OutputName" -ForegroundColor Green
    } else {
        Write-Host "✗ $RID build FAILED (exit code: $exitCode)" -ForegroundColor Red
        exit $exitCode
    }
}

# Main logic
switch ($Target) {
    'win' {
        Build-RID -RID 'win-x64' -OutputName 'win-x64'
    }
    'mac' {
        Build-RID -RID 'osx-x64' -OutputName 'osx-x64'
        Build-RID -RID 'osx-arm64' -OutputName 'osx-arm64'
    }
    'linux' {
        Build-RID -RID 'linux-x64' -OutputName 'linux-x64'
    }
    'all' {
        # Detect current platform
        $rid = dotnet --info | Select-String 'RID' | ForEach-Object { $_.Line.Split(':')[1].Trim() }
        Write-Host "Current platform RID: $rid" -ForegroundColor Cyan
        
        switch -Wildcard ($rid) {
            'win-x64' { Build-RID -RID 'win-x64' -OutputName 'win-x64' }
            'osx-*' { Build-RID -RID $rid -OutputName $rid }
            'linux-x64' { Build-RID -RID 'linux-x64' -OutputName 'linux-x64' }
            default {
                Write-Host "Unknown RID: $rid" -ForegroundColor Red
                exit 1
            }
        }
    }
    default {
        Write-Host "Usage: .\build.ps1 [-Target <win|mac|linux|all>]" -ForegroundColor Red
        Write-Host "  win    - Build Windows x64"
        Write-Host "  mac    - Build macOS x64 + ARM64"
        Write-Host "  linux  - Build Linux x64"
        Write-Host "  all    - Build for current platform (default)"
        exit 1
    }
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Build complete! Output in $OutputDir/" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Get-ChildItem "$OutputDir" -Recurse | Where-Object { -not $_.PSIsContainer } | Select-Object Name, Length, FullName | Format-Table -AutoSize