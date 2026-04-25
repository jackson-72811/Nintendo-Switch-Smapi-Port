#!/usr/bin/env bash
# =============================================================================
# SMAPI Switch — Linux Build Script
# Requires:
#   - devkitPro (devkitA64 + libnx)  — set $DEVKITPRO or install to /opt/devkitpro
#   - .NET 8 SDK                      — dotnet must be in PATH
#   - CMake 3.20+                     — cmake must be in PATH
#   - make                            — standard GNU make
# =============================================================================
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "============================================================"
echo " SMAPI Switch Build System (Linux)"
echo "============================================================"
echo

# ── Locate devkitPro ─────────────────────────────────────────────────────────
if [[ -z "${DEVKITPRO:-}" ]]; then
    if [[ -d /opt/devkitpro/devkitA64 ]]; then
        export DEVKITPRO=/opt/devkitpro
    else
        echo "[ERROR] devkitPro not found. Set \$DEVKITPRO or install to /opt/devkitpro."
        echo "        Install guide: https://devkitpro.org/wiki/Getting_Started"
        exit 1
    fi
fi
echo "[INFO] Using devkitPro at: $DEVKITPRO"
export PATH="$DEVKITPRO/devkitA64/bin:$DEVKITPRO/tools/bin:$PATH"

# ── Check .NET SDK ─────────────────────────────────────────────────────────────
if ! command -v dotnet &>/dev/null; then
    echo "[ERROR] dotnet not found. Install the .NET 8 SDK:"
    echo "        https://dotnet.microsoft.com/download"
    exit 1
fi
echo "[INFO] .NET SDK: $(dotnet --version)"

# ── Check CMake ────────────────────────────────────────────────────────────────
if ! command -v cmake &>/dev/null; then
    echo "[ERROR] cmake not found. Install with: sudo apt install cmake"
    exit 1
fi
echo "[INFO] $(cmake --version | head -1)"
echo

# ── Create output directories ──────────────────────────────────────────────────
mkdir -p "$SCRIPT_DIR/build/native"
mkdir -p "$SCRIPT_DIR/build/managed"
mkdir -p "$SCRIPT_DIR/build/deploy"

# =============================================================================
# Step 1 — Build managed code (.NET)
# =============================================================================
echo "[STEP 1/3] Building managed SMAPI assemblies..."
(
    cd "$SCRIPT_DIR/managed"
    dotnet restore SMAPI.Switch.sln
    dotnet build   SMAPI.Switch.sln -c Release -o "$SCRIPT_DIR/build/managed"
)
echo "[INFO] Managed build succeeded."
echo

# =============================================================================
# Step 2 — Build native plugin (aarch64-none-elf)
# =============================================================================
echo "[STEP 2/3] Building native sysmodule (aarch64-none-elf)..."
(
    cd "$SCRIPT_DIR/build/native"
    cmake "$SCRIPT_DIR" \
        -DCMAKE_TOOLCHAIN_FILE="$SCRIPT_DIR/toolchain-switch.cmake" \
        -DCMAKE_BUILD_TYPE=Release
    cmake --build . --config Release --parallel "$(nproc)"
)
echo "[INFO] Native build succeeded."
echo

# =============================================================================
# Step 3 — Assemble deployment package
# =============================================================================
echo "[STEP 3/3] Assembling SD card deployment package..."

DEPLOY="$SCRIPT_DIR/build/deploy"
CONTENTS="$DEPLOY/atmosphere/contents/0100E65002BB8000"

mkdir -p "$CONTENTS/exefs"
mkdir -p "$CONTENTS/romfs/smapi-internal"
mkdir -p "$CONTENTS/romfs/Mods"
mkdir -p "$DEPLOY/switch/smapi/Mods"

if [[ -f "$SCRIPT_DIR/build/native/smapi_switch.nro" ]]; then
    cp "$SCRIPT_DIR/build/native/smapi_switch.nro" "$CONTENTS/exefs/smapi_switch.nro"
    echo "[INFO] Copied smapi_switch.nro"
else
    echo "[WARN] smapi_switch.nro not found — native build may have failed silently."
fi

cp "$SCRIPT_DIR/build/managed/"*.dll "$CONTENTS/romfs/smapi-internal/" 2>/dev/null || true
cp "$SCRIPT_DIR/build/managed/"*.pdb "$CONTENTS/romfs/smapi-internal/" 2>/dev/null || true
cp "$SCRIPT_DIR/deploy/README.txt"   "$DEPLOY/README.txt" 2>/dev/null || true

echo
echo "============================================================"
echo " Build complete!  Output in: build/deploy/"
echo
echo " SD card layout:"
echo "   atmosphere/contents/0100E65002BB8000/exefs/smapi_switch.nro"
echo "   atmosphere/contents/0100E65002BB8000/romfs/smapi-internal/*.dll"
echo "   switch/smapi/Mods/    <-- put your mods here"
echo "============================================================"
