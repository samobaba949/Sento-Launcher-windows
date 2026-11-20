#!/bin/bash
# create-linux-appimage.sh - Creates AppImage for Linux
# Run on Linux after: dotnet publish -r linux-x64 --self-contained true

set -e

APP_NAME="SentoLauncher"
VERSION="1.0.1"
PUBLISH_DIR="publish/linux-x64"
APPDIR="release/${APP_NAME}.AppDir"
APPIMAGE="release/${APP_NAME}-${VERSION}-linux-x64.AppImage"

echo "Creating Linux AppImage..."

# Clean previous
rm -rf "${APPDIR}" "${APPIMAGE}"

# Create AppDir structure
mkdir -p "${APPDIR}/usr/bin"
mkdir -p "${APPDIR}/usr/share/applications"
mkdir -p "${APPDIR}/usr/share/icons/hicolor/256x256/apps"

# Copy executable
cp "${PUBLISH_DIR}/SentoLauncher" "${APPDIR}/usr/bin/"
chmod +x "${APPDIR}/usr/bin/SentoLauncher"

# Copy all published files
cp -r "${PUBLISH_DIR}/"* "${APPDIR}/usr/bin/"

# Create .desktop file
cat > "${APPDIR}/usr/share/applications/sentolauncher.desktop" << EOF
[Desktop Entry]
Type=Application
Name=Sento Launcher
Comment=Minecraft Launcher with Mod Support
Exec=SentoLauncher
Icon=sentolauncher
Terminal=false
Categories=Game;Application;
StartupNotify=true
StartupWMClass=SentoLauncher
Keywords=Minecraft;Launcher;Mods;Fabric;Forge;
EOF

# Copy icon (convert if needed)
if [ -f "Assets/icon.png" ]; then
    cp "Assets/icon.png" "${APPDIR}/usr/share/icons/hicolor/256x256/apps/sentolauncher.png"
elif [ -f "Assets/icon.ico" ]; then
    # Convert using ImageMagick if available
    convert "Assets/icon.ico" -resize 256x256 "${APPDIR}/usr/share/icons/hicolor/256x256/apps/sentolauncher.png" 2>/dev/null || true
fi

# Copy icon to AppDir root for AppImage
cp "${APPDIR}/usr/share/icons/hicolor/256x256/apps/sentolauncher.png" "${APPDIR}/sentolauncher.png" 2>/dev/null || true

# Create AppRun script
cat > "${APPDIR}/AppRun" << 'EOF'
#!/bin/bash
# AppRun - Entry point for AppImage
HERE="$(dirname "$(readlink -f "${BASH_SOURCE[0]}")")"
export PATH="${HERE}/usr/bin:${PATH}"
export LD_LIBRARY_PATH="${HERE}/usr/lib:${LD_LIBRARY_PATH}"

# Find the actual executable
EXEC="${HERE}/usr/bin/SentoLauncher"
if [ -f "${EXEC}" ]; then
    exec "${EXEC}" "$@"
else
    echo "Error: Executable not found at ${EXEC}"
    exit 1
fi
EOF
chmod +x "${APPDIR}/AppRun"

# Download appimagetool if not present
if [ ! -f "appimagetool-x86_64.AppImage" ]; then
    echo "Downloading appimagetool..."
    wget -q "https://github.com/AppImage/AppImageKit/releases/download/continuous/appimagetool-x86_64.AppImage" -O appimagetool-x86_64.AppImage
    chmod +x appimagetool-x86_64.AppImage
fi

# Build AppImage
echo "Building AppImage..."
ARCH=x86_64 ./appimagetool-x86_64.AppImage "${APPDIR}" "${APPIMAGE}"

if [ -f "${APPIMAGE}" ]; then
    echo "✓ AppImage created: ${APPIMAGE}"
    ls -lh "${APPIMAGE}"
else
    echo "✗ AppImage creation failed"
    exit 1
fi