# Network Extensions 2 (Modernized) v2.1.0

A modernized version of the successful **Network Extensions 2** mod for Cities: Skylines, updated for version **1.21+** and optimized for both Windows and Linux environments.
[Steam workshop page](https://steamcommunity.com/sharedfiles/filedetails/?id=812125426&searchtext=network+extentions+2)

## 🚀 Key Features

- **Modern DLC Support**: Full integration with modern DLC road categories:
    - 🚋 **Trams**
    - 🚟 **Monorail**
    - 🚎 **Trolleybus**
- **Linux Compatibility**: Standardized cross-platform build process and case-sensitivity fixes.
- **Robust Asset Loading**: Uses `PluginInfo.modPath` for reliable resolution of 3D models and textures on all platforms.
- **CSM Support**: Verified for use with the [Cities: Skylines Multiplayer (CSM)](https://github.com/CSM-Developers/CSM) mod (requires the `CSM.TmpeSync` bridge).
- **Integrated Harmony API**: Removed dependency on external Harmony API DLLs for a smoother installation.

## 🛠️ Build Instructions (Linux)

I have provided a `build-linux.sh` script to automate the entire process:

1.  **Dependencies**: Place your game DLLs (ICities.dll, UnityEngine.dll, etc.) in a predictable path or update the script to point to your game installation.
2.  **Run Build**:
    ```bash
    chmod +x build-linux.sh
    ./build-linux.sh
    ```
    *This script will compile the project and bundle all assets (`Buildings`, `Props`, `Roads`, `Menus`, and `*.crp` files) into the `bin/Release` folder.*

## 📖 Dependencies & Recommended Mods

- **Traffic Manager: President Edition (TM:PE)**: We highly recommend using the latest version of [TM:PE](https://github.com/CitiesSkylinesMods/TMPE) for advanced traffic control.
- **CitiesHarmony**: This mod uses Harmony 2.2.2+ for patching.

## 📝 License

This project is a continuation of the Network Extensions Project. All original licenses and credits to the original team apply.
