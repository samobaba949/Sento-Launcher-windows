#!/bin/bash
# build-all.sh - Cross-platform build script for Sento Launcher
# Run on each target platform (Windows, macOS, Linux) or use GitHub Actions

set -e

PROJECT="SentoLauncher.csproj"
CONFIG="Release"
OUTPUT_DIR="publish"

echo "========================================="
echo "Sento Launcher - Cross-Platform Build"
echo "========================================="

# Function to build for a specific RID
build_rid() {
    local RID=$1
    local OUTPUT_NAME=$2
    
    echo ""
    echo "Building for $RID..."
    echo "-----------------------------------------"
    
    dotnet publish "$PROJECT" \
        -c "$CONFIG" \
        -r "$RID" \
        --self-contained true \
        -p:PublishSingleFile=true \
        -p:PublishTrimmed=true \
        -p:TrimMode=partial \
        -o "$OUTPUT_DIR/$OUTPUT_NAME" \
        --no-restore
    
    if [ $? -eq 0 ]; then
        echo "✓ $RID build successful: $OUTPUT_DIR/$OUTPUT_NAME"
    else
        echo "✗ $RID build FAILED"
        exit 1
    fi
}

# Parse arguments
case "${1:-all}" in
    win|windows|win-x64)
        build_rid "win-x64" "win-x64"
        ;;
    mac|macos|osx)
        build_rid "osx-x64" "osx-x64"
        build_rid "osx-arm64" "osx-arm64"
        ;;
    linux|linux-x64)
        build_rid "linux-x64" "linux-x64"
        ;;
    all)
        echo "Building all platforms..."
        # Note: This will only work on the current platform
        # Use GitHub Actions for true cross-platform builds
        CURRENT_RID=$(dotnet --info | grep "RID" | head -1 | awk '{print $2}')
        echo "Current platform RID: $CURRENT_RID"
        
        case "$CURRENT_RID" in
            win-x64)
                build_rid "win-x64" "win-x64"
                ;;
            osx-x64|osx-arm64)
                build_rid "$CURRENT_RID" "$CURRENT_RID"
                ;;
            linux-x64)
                build_rid "linux-x64" "linux-x64"
                ;;
            *)
                echo "Unknown RID: $CURRENT_RID"
                exit 1
                ;;
        esac
        ;;
    *)
        echo "Usage: $0 [win|mac|linux|all]"
        echo "  win    - Build Windows x64"
        echo "  mac    - Build macOS x64 + ARM64"
        echo "  linux  - Build Linux x64"
        echo "  all    - Build for current platform"
        exit 1
        ;;
esac

echo ""
echo "========================================="
echo "Build complete! Output in $OUTPUT_DIR/"
echo "========================================="
ls -la "$OUTPUT_DIR"/