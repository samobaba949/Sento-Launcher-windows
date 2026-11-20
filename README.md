# Sento Launcher

Modern, Horizon-style Minecraft launcher built with C# WPF (.NET 8) featuring Microsoft OAuth 2.0 authentication, offline mode support, and ultra-low bandwidth architecture.

## ✨ Features

- **🎮 Dual Authentication**: Microsoft OAuth 2.0 (official) + Offline mode (local UUID generation)
- **⚡ Zero-Bandwidth Launch**: Offline-first architecture - only downloads version manifest, uses local files if complete
- **🔒 Secure Local Storage**: Encrypted config using Windows DPAPI (`%appdata%/.sentolauncher/config.json`)
- **🎨 Horizon-Style UI**: Dark minimalist theme (#0D0D11), custom title bar, rounded corners, smooth animations
- **🚀 Optimized JVM Args**: G1GC tuned for Ryzen 3, high process priority, parallel GC
- **📦 Version Management**: Local version scanning + remote manifest fetching
- **💾 Persistent Settings**: RAM, Java path, JVM args, window size, game directory

## 📋 Requirements

- **OS**: Windows 10/11 (x64)
- **Runtime**: .NET 8 Desktop Runtime
- **Java**: Java 17+ (for Minecraft 1.18+) or Java 8 (for older versions)
- **Minecraft**: Existing installation or will download on first launch

## 🛠 Building

### Prerequisites

1. Install [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Install [Visual Studio 2022](https://visualstudio.microsoft.com/) with **.NET desktop development** workload
   - Or use VS Code with C# Dev Kit extension

### Build Commands

```bash
# Clone and build
git clone <repo-url>
cd SentoLauncher

# Restore packages
dotnet restore

# Build Debug
dotnet build -c Debug

# Build Release
dotnet build -c Release

# Run
dotnet run --project SentoLauncher.csproj
```

### Publish (Single File)

```bash
# Self-contained single file (larger, no runtime needed)
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ./publish

# Framework-dependent (smaller, requires .NET 8 Runtime)
dotnet publish -c Release -r win-x64 --self-contained false -o ./publish
```

## 📦 NuGet Dependencies

```xml
<PackageReference Include="CMLib.Core" Version="1.0.0" />
<PackageReference Include="CMLib.Core.Auth.Microsoft" Version="1.0.0" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
<PackageReference Include="System.Text.Encodings.Web" Version="8.0.0" />
```

> **Note**: `CMLib.Core` and `CMLib.Core.Auth.Microsoft` are placeholder package names. Replace with actual Minecraft authentication libraries like:
> - `MinecraftLaunch` / `MinecraftLaunch.Components`
> - `CMFL.MVVM` / `CMFL.Auth.Microsoft`
> - Or implement custom authentication (code includes full Microsoft OAuth 2.0 flow)

## 🏗 Architecture

```
SentoLauncher/
├── App.xaml / App.xaml.cs           # Application entry, single-instance, DI
├── MainWindow.xaml / .cs            # Main UI (Horizon style), navigation, launch logic
├── Styles/
│   ├── Colors.xaml                  # Color palette, brushes, gradients
│   └── Controls.xaml                # Control templates, styles, converters
├── Converters.cs                    # Value converters (BoolToVis, Slider, etc.)
├── Models/
│   └── LauncherModels.cs            # Data models (Account, Version, Config, etc.)
├── Services/
│   ├── ConfigManager.cs             # Encrypted config (DPAPI), settings persistence
│   ├── MicrosoftAuthService.cs      # Full OAuth 2.0 flow (Auth → XBL → XSTS → MC)
│   ├── OfflineAuthService.cs        # Offline UUID generation, username validation
│   ├── GameLaunchService.cs         # JVM args, classpath, process launch
│   └── VersionManager.cs            # Version scanning, manifest, integrity check
└── Assets/
    ├── icon.ico                     # Window icon
    └── logo.png                     # Branding
```

## 🔐 Microsoft OAuth Flow

```
1. Browser opens: login.microsoftonline.com (Client ID: Minecraft Launcher)
2. User signs in → redirect to http://localhost:8080/callback?code=...
3. Local HTTP listener captures auth code
4. Exchange code → Microsoft access/refresh tokens
5. Microsoft token → Xbox Live (XBL) token
6. XBL token → XSTS token (RETAIL sandbox)
7. XSTS token → Minecraft access token
8. Fetch profile (UUID, name, skins)
9. Save encrypted to config.json
```

## ⚙️ Configuration

Config stored at: `%appdata%/.sentolauncher/config.json`

Encrypted with **Windows DPAPI** (user-scoped) - only your user account can decrypt.

### Key Settings

| Setting | Default | Description |
|---------|---------|-------------|
| `RamMB` | 4096 | Heap size (1-16 GB) |
| `JavaPath` | Auto | Path to `javaw.exe` |
| `JvmArguments` | Optimized G1GC | Custom JVM flags |
| `GameDirectory` | `%appdata%/.minecraft` | Minecraft root |
| `PriorityClass` | High | Process priority |
| `AutoCloseLauncher` | true | Close on launch |

## 🎯 JVM Arguments (Default)

Optimized for **Ryzen 3 / modern CPUs**:

```bash
-XX:+UseG1GC
-XX:+ParallelRefProcEnabled
-XX:MaxGCPauseMillis=50
-XX:+UnlockExperimentalVMOptions
-XX:+DisableExplicitGC
-XX:+AlwaysPreTouch
-XX:G1NewSizePercent=30
-XX:G1MaxNewSizePercent=40
-XX:G1HeapRegionSize=8M
-XX:G1ReservePercent=20
-XX:G1HeapWastePercent=5
-XX:G1MixedGCCountTarget=4
-XX:InitiatingHeapOccupancyPercent=15
-XX:G1MixedGCLiveThresholdPercent=90
-XX:G1RSetUpdatingPauseTimePercent=5
-XX:SurvivorRatio=32
-XX:+PerfDisableSharedMem
-XX:MaxTenuringThreshold=1
```

## 📱 UI Overview

### Main Window (1050×650)
- **Custom Title Bar**: Logo, version, minimize/maximize/close
- **Sidebar (260px)**: Navigation + user avatar/status
- **Content Area**: Three panels (Play / Account / Settings)

### Play Panel
- Version dropdown (local first, shows install status)
- RAM slider (1-16 GB, snap to 1 GB)
- Java path selector
- **SENTO OYNA** button with progress overlay

### Account Panel
- **Current Account Card**: Avatar, name, type, logout
- **Tabs**: 
  - Çevrimdışı (username input + login)
  - Microsoft (OAuth button + status)
- **Saved Accounts List**: Switch/remove accounts

### Settings Panel
- Java & Performance (path, JVM args, priority, auto-close)
- Game Directory (browse, window size, fullscreen)
- About

## 🔧 Development

### Adding Features

1. **New Panel**: Add to `MainContentArea` Grid, toggle visibility in navigation
2. **New Setting**: Add to `LauncherSettings` model, bind in Settings panel, persist in `SaveSettingsFromUI`
3. **New Auth Type**: Extend `AuthType` enum, update converters, add tab

### Debugging

```bash
# Enable debug logging
set DOTNET_ENVIRONMENT=Development
dotnet run

# View logs in Output window (Debug → Windows → Output)
```

## 📄 License

MIT License - See LICENSE file for details.

## 🤝 Contributing

1. Fork the repository
2. Create feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open Pull Request

## 🙏 Acknowledgments

- **Horizon Launcher** - UI/UX inspiration
- **Minecraft Community** - Protocol documentation
- **CMLib / MinecraftLaunch** - Authentication reference
- **WPF** - Framework

---

**Sento Launcher** - Built with ❤️ for the Minecraft community