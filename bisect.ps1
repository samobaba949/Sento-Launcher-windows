param()
$ErrorActionPreference = 'Continue'
$root = 'G:\Sento Launcher'
$origPath = Join-Path $root 'Styles\Controls.xaml.orig'
$controlsPath = Join-Path $root 'Styles\Controls.xaml'
if (-not (Test-Path $origPath)) { Copy-Item $controlsPath $origPath -Force }
$orig = Get-Content $origPath

# Top-level sections (1-based line numbers of each group's first style start line)
$groupStarts = [ordered]@{
  g1  = 64    # Thumb + ScrollBar + ScrollViewer
  g2  = 163   # MainWindowStyle
  g3  = 175   # TitleBar + Close buttons
  g4  = 222   # Sidebar + SidebarActive
  g5  = 293   # Primary + Secondary + Accent buttons
  g6  = 422   # TextBox + PasswordBox
  g7  = 506   # ComboBoxItem + ComboBox
  g8  = 615   # Slider + ProgressBar
  g9  = 728   # Card + TabItem + TabControl
  g10 = 818   # CheckBox
  g11 = 878   # Avatar + Tooltip + Separator
}
$allKeys = @($groupStarts.Keys)
$nextStart = @{}
for ($i = 0; $i -lt $allKeys.Count - 1; $i++) { $nextStart[$allKeys[$i]] = $groupStarts[$allKeys[$i+1]] }
$header = $orig[0..61]
$footer = '</ResourceDictionary>'
$fileEnd = $orig.Count

function Build-Subset([string[]]$groups) {
  $bodyLines = @()
  foreach ($g in $groups) {
    $start = $groupStarts[$g]
    if ($nextStart.ContainsKey($g)) { $end = $nextStart[$g] - 1 } else { $end = $fileEnd - 1 }
    # include preceding comment lines of this group: scan back while line is a comment/blank that follows </Style>
    $bodyLines += $orig[($start - 1)..($end - 1)]
  }
  ($header + $bodyLines + $footer) | Set-Content $controlsPath -Encoding UTF8
}

function Run-Test([string[]]$groups) {
  Build-Subset $groups
  $null = Get-Process SentoLauncher -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
  Start-Sleep -Milliseconds 500
  $env:DOTNET_ROOT = 'G:\dotnet'; $env:PATH = 'G:\dotnet;' + $env:PATH
  $env:NUGET_PACKAGES = 'G:\nuget'; $env:DOTNET_CLI_HOME = 'G:\dotnet-home'
  $env:TEMP = 'G:\temp'; $env:TMP = 'G:\temp'
  $build = dotnet build (Join-Path $root 'SentoLauncher.csproj') -c Release -p:NoWarn=CS8604 2>&1
  if ($build -match 'error') { return 'BUILDERR' }
  Remove-Item (Join-Path $root 'bin\Release\net8.0-windows\win-x64\crash.log') -ErrorAction SilentlyContinue
  Start-Process (Join-Path $root 'bin\Release\net8.0-windows\win-x64\SentoLauncher.exe')
  Start-Sleep -Seconds 6
  $null = Get-Process SentoLauncher -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
  Start-Sleep -Milliseconds 500
  $log = Join-Path $root 'bin\Release\net8.0-windows\win-x64\crash.log'
  if (-not (Test-Path $log)) { return 'OK' }
  $content = Get-Content $log -Raw
  if ($content -match "Parameter 'property'") { return 'SETTERFAIL' }
  if ($content -match 'Cannot find|bulunamad') { return 'DEPFAIL' }
  return 'OTHER: ' + ($content -split "`n")[1]
}

function Show-Set($g) { ($g -join ',') }

# Bisection for a single failing group
$set = $allKeys
Write-Host ("FULL SET: " + (Run-Test $set))
while ($set.Count -gt 1) {
  $half = [int][Math]::Ceiling($set.Count / 2)
  $part1 = $set[0..($half - 1)]
  Write-Host ("TEST [" + (Show-Set $part1) + "]:")
  $r1 = Run-Test $part1
  Write-Host ("  => $r1")
  if ($r1 -eq 'SETTERFAIL') { $set = $part1; continue }
  $part2 = $set[$half..($set.Count - 1)]
  Write-Host ("TEST [" + (Show-Set $part2) + "]:")
  $r2 = Run-Test $part2
  Write-Host ("  => $r2")
  if ($r2 -eq 'SETTERFAIL') { $set = $part2; continue }
  break
}
Write-Host ("CULPRIT GROUP(S): " + (Show-Set $set))
