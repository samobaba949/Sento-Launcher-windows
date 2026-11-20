#!/bin/bash
# create-macos-app.sh - Creates .app bundle for macOS
# Run on macOS after: dotnet publish -r osx-x64 --self-contained true

set -e

APP_NAME="SentoLauncher"
VERSION="1.0.1"
RID="${1:-osx-x64}"  # osx-x64 or osx-arm64
PUBLISH_DIR="publish/${RID}"
APP_DIR="release/${APP_NAME}.app"

echo "Creating macOS app bundle for ${RID}..."

# Clean previous
rm -rf "${APP_DIR}"

# Create app bundle structure
mkdir -p "${APP_DIR}/Contents/MacOS"
mkdir -p "${APP_DIR}/Contents/Resources"

# Copy executable
cp "${PUBLISH_DIR}/SentoLauncher" "${APP_DIR}/Contents/MacOS/"
chmod +x "${APP_DIR}/Contents/MacOS/SentoLauncher"

# Copy icon (convert .ico to .icns first if needed)
if [ -f "Assets/icon.icns" ]; then
    cp "Assets/icon.icns" "${APP_DIR}/Contents/Resources/AppIcon.icns"
elif [ -f "Assets/icon.ico" ]; then
    # Convert using sips (macOS built-in)
    sips -s format icns Assets/icon.ico --out "${APP_DIR}/Contents/Resources/AppIcon.icns" 2>/dev/null || true
fi

# Create Info.plist
cat > "${APP_DIR}/Contents/Info.plist" << EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleName</key>
    <string>${APP_NAME}</string>
    <key>CFBundleDisplayName</key>
    <string>Sento Launcher</string>
    <key>CFBundleIdentifier</key>
    <string>com.sento.launcher</string>
    <key>CFBundleVersion</key>
    <string>${VERSION}</string>
    <key>CFBundleShortVersionString</key>
    <string>${VERSION}</string>
    <key>CFBundleExecutable</key>
    <string>SentoLauncher</string>
    <key>CFBundleIconFile</key>
    <string>AppIcon</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleSignature</key>
    <string>SENT</string>
    <key>LSMinimumSystemVersion</key>
    <string>10.15</string>
    <key>NSHighResolutionCapable</key>
    <true/>
    <key>NSRequiresAquaSystemAppearance</key>
    <false/>
    <key>LSApplicationCategoryType</key>
    <string>public.app-category.games</string>
    <key>CFBundleDocumentTypes</key>
    <array/>
    <key>UTExportedTypeDeclarations</key>
    <array/>
</dict>
</plist>
EOF

# Create PkgInfo
echo "APPLSENT" > "${APP_DIR}/Contents/PkgInfo"

echo "✓ App bundle created: ${APP_DIR}"
echo ""
echo "To create DMG installer:"
echo "  hdiutil create -volname \"Sento Launcher\" -srcfolder \"${APP_DIR}\" -ov -format UDZO \"release/SentoLauncher-${VERSION}-${RID}.dmg\""