#!/bin/bash

# Configuration
DEFAULT_GAME_PATH="/var/mnt/schijven/1TB SSD/Games/Cities - Skylines/drive_c/Program Files (x86)/Cities.Skylines.v1.21.1.F5"

# Environment override
if [ -n "$CITIES_SKYLINES_DIR" ]; then
    GAME_PATH="$CITIES_SKYLINES_DIR"
else
    GAME_PATH="$DEFAULT_GAME_PATH"
fi

echo "=== Building Network Extensions 2 ==="
echo "Game path: $GAME_PATH"

# Check if game path exists
if [ ! -d "$GAME_PATH" ]; then
    echo "Error: Game path not found at $GAME_PATH"
    echo "Please set CITIES_SKYLINES_DIR environment variable."
    exit 1
fi

# Create References directory
mkdir -p References

# Setup symlinks for game DLLs
echo "Setting up dependencies..."
ln -sf "$GAME_PATH/Cities_Data/Managed/UnityEngine.dll" References/UnityEngine.dll
ln -sf "$GAME_PATH/Cities_Data/Managed/Assembly-CSharp.dll" References/Assembly-CSharp.dll
ln -sf "$GAME_PATH/Cities_Data/Managed/ColossalManaged.dll" References/ColossalManaged.dll
ln -sf "$GAME_PATH/Cities_Data/Managed/ICities.dll" References/ICities.dll

# Setup Harmony dependencies
HARMONY_DIR="$GAME_PATH/Files/Mods/Harmony 2.2.2-0"
if [ -d "$HARMONY_DIR" ]; then
    echo "Found Harmony folder at: $HARMONY_DIR"
    ln -sf "$HARMONY_DIR/CitiesHarmony.Harmony.dll" References/CitiesHarmony.Harmony.dll
    # Usually CitiesHarmony.dll is the API
    if [ -f "$HARMONY_DIR/CitiesHarmony.dll" ]; then
        ln -sf "$HARMONY_DIR/CitiesHarmony.dll" References/CitiesHarmony.API.dll
    fi
else
    echo "Warning: Harmony folder not found!"
fi

# 1. Build ObjUnity3D
echo "--- Building ObjUnity3D ---"
dotnet build "ObjUnity3D/ObjUnity3D.csproj" -c Release
if [ $? -eq 0 ]; then
    cp "ObjUnity3D/bin/Release/ObjUnity3D.dll" "References/ObjUnity3D.dll"
else
    echo "Error: ObjUnity3D build failed!"
    exit 1
fi

# 2. Build NetworkExtensions2
echo "--- Building NetworkExtensions2 ---"
dotnet build "NetworkExtensions2/NetworkExtensions2.csproj" -c Release

if [ $? -eq 0 ]; then
    echo "=== Build Succeeded! ==="
    DLL_PATH="NetworkExtensions2/bin/Release/NetworkExtensions2.dll"
    OUTPUT_DIR="NetworkExtensions2/bin/Release"

    # --- Asset Bundling ---
    echo "Bundling assets..."
    # Copy any .crp files from root to output
    cp *.crp "$OUTPUT_DIR/" 2>/dev/null || true

    # Copy asset directories from root to output (if they were placed there by the user)
    ASSET_FOLDERS=("Buildings" "Props" "Roads" "Menus" "Resources")
    for folder in "${ASSET_FOLDERS[@]}"; do
        if [ -d "$folder" ]; then
            echo "Copying asset folder: $folder"
            cp -r "$folder" "$OUTPUT_DIR/"
        fi
    done

    echo "DLL located at: $DLL_PATH"
    echo "Note: Ensure you have copied the *.crp files and asset folders from the Steam Workshop to the root directory for a complete build."
else
    echo "=== Build Failed ==="
    exit 1
fi
