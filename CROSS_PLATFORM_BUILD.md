# Sento Launcher - Cross-Platform Build Guide

## Platform Support

| Platform | Architectures | Status | Package Format |
|----------|---------------|--------|----------------|
| Windows  | x64           | ✅ Tested | Single-file EXE (self-contained) |
| macOS    | x64, ARM64    | ⚠️ Code Ready | .app bundle + DMG |
| Linux    | x64           | ⚠️ Code Ready | AppImage + .desktop |

## Prerequisites

- **.NET 8.0 SDK** installed
- Platform-specific dependencies:
  - **Windows**: None (built-in)
  - **macOS**: Xcode Command Line Tools (`xcode-select --install`)
  - **Linux**: GTK3 + WebKitGTK (`sudo apt install libgtk-3-0 libwebkit2gtk-4.0-0`)

## Quick Build Commands

### Windows (run on Windows)
```powershell
# Build for current platform
.\build.ps1 -Target win

# Or use bash
./build.sh win
```

### macOS (run on macOS)
```bash
# Build for Intel + Apple Silicon
./build.sh mac

# Create .app bundle + DMG
./create-macos-app.sh osx-x64
./create-macos-app.sh osx-arm64
# Then create DMG:
hdiutil create -volname "Sento Launcher" -srcfolder "release/SentoLauncher.app" -ov -format UDZO "release/SentoLauncher-1.0.1-macos.dmg"
```

### Linux (run on Linux)
```bash
# Build
./build.sh linux

# Create AppImage
./create-linux-appimage.sh

# Install system-wide
sudo cp sentolauncher.desktop /usr/share/applications/
sudo cp Assets/icon.png /usr/share/icons/hicolor/256x256/apps/sentolauncher.png
```

## Automated Cross-Platform Builds (GitHub Actions)

The `.github/workflows/build.yml` handles all three platforms automatically:

1. **Push a tag**: `git tag v1.0.1 && git push origin v1.0.1`
2. **Workflow triggers** on tag push
3. **Builds** on Windows, macOS (x64 + ARM64), Linux
3. **Creates GitHub Release** with:
   - `SentoLauncher-win-x64.zip` (Windows EXE)
   - `SentoLauncher-osx-x64.zip` + `SentoLauncher-osx-arm64.zip` (macOS app bundles)
   - `SentoLauncher-linux-x64.tar.gz` (Linux AppImage)
   - `SHA256SUMS.txt` for verification

## Platform-Specific Code Paths

The codebase uses `OperatingSystem.IsWindows()`, `IsMacOS()`, `IsLinux()` for platform detection:

| Feature | Windows | macOS | Linux |
|---------|---------|-------|-------|
| Java executable | `javaw.exe` | `java` | `java` |
| Config directory | `%APPDATA%\sentolauncher` | `~/Library/Application Support/sentolauncher` | `~/.local/share/sentolauncher` |
| Minecraft directory | `%APPDATA%\.minecraft` | `~/Library/Application Support/minecraft` | `~/.minecraft` |
| Java installations | `ProgramFiles\Java` | `/Library/Java/JavaVirtualMachines` | `/usr/lib/jvm` |
| GPU preference (AMD) | Registry ✓ | N/A | N/A |
| Encryption | DPAPI (legacy) + AES-GCM | AES-GCM | AES-GCM |
| Process priority | ✓ | N/A | N/A |
| Installer (Forge/NeoForge) | ✓ | ✓ (Java-based) | ✓ (Java-based) |

## Distribution Checklist

### Windows (✅ Ready)
- [x] Self-contained single EXE
- [x] No installer needed (portable)
- [x] Code signing ready (add cert to workflow)

### macOS (⚠️ Needs Testing)
- [ ] Test on Intel Mac (x64)
- [ ] Test on Apple Silicon (ARM64)
- [ ] Notarization workflow (Apple Developer account needed)
- [ ] DMG with proper background/layout

### Linux (⚠️ Needs Testing)
- [ ] Test on Ubuntu 22.04+, Fedora, Arch
- [ ] AppImage runs on Wayland + X11
- [ ] .desktop file installs correctly
- [ ] MIME type registration for `sentolauncher://` URLs

## Known Platform Issues

| Issue | Platform | Workaround |
|-------|----------|------------|
| Forge/NeoForge installers | macOS/Linux | Use Java-based installer (works) |
| GPU preference (AMD crash fix) | Windows only | N/A on other platforms |
| DPAPI encryption | Windows only | AES-GCM fallback on other platforms |
| System tray / notifications | Windows only | Not implemented for macOS/Linux |

## Manual Verification Commands

```bash
# Verify builds are self-contained
dotnet publish -c Release -r win-x64 --self-contained true -o test/win
dotnet publish -c Release -r osx-x64 --self-contained true -o test/mac
dotnet publish -c Release -r linux-x64 --self-contained true -o test/linux

# Check file sizes
ls -lh test/*/SentoLauncher*

# Test run (on each platform)
test/win/SentoLauncher.exe
test/mac/SentoLauncher
test/linux/SentoLauncher
```