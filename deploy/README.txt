SMAPI Switch — SD Card Installation Instructions
================================================

Requirements
------------
- Nintendo Switch with Atmosphere CFW (14.0+)
- Stardew Valley (Title ID: 0100E65002BB8000)

Installation
------------
Copy the contents of build\deploy\ to the root of your SD card:

  SD:/
  ├── atmosphere/
  │   └── contents/
  │       └── 0100E65002BB8000/    ← Stardew Valley title override
  │           ├── exefs/
  │           │   └── smapi_switch.nro   ← Native sysmodule
  │           └── romfs/
  │               ├── smapi-internal/    ← SMAPI managed assemblies
  │               └── Mods/              ← Game-bundled mods (optional)
  └── switch/
      └── smapi/
          ├── Mods/                ← Install your SMAPI mods here
          └── smapi-switch.log     ← Created at runtime

Installing Mods
---------------
1. Build (or download) a SMAPI mod as usual.
2. Copy the mod folder (containing manifest.json) into:
   SD:/switch/smapi/Mods/YourModName/

The mod must target .NET 8 (net8.0) or be a .NET Framework mod that
runs under the Switch's embedded Mono runtime.

Notes
-----
- Only mods that are pure managed code will work.  Mods that ship
  native Windows DLLs (e.g. SQLite P/Invoke) need ARM64 replacements.
- Content packs (xnb replacements, JSON data packs) work as-is.
- The save-data folder is at SD:/switch/smapi/save-data/

Log file
--------
  SD:/switch/smapi/smapi-switch.log

Troubleshooting
---------------
If the game crashes on startup, check smapi-switch.log for details.
Common issues:
  - Mod requires a newer API version than installed
  - Mod DLL was compiled for x64 instead of AnyCPU
  - Missing HarmonyX or other NuGet dependency in the mod's folder
