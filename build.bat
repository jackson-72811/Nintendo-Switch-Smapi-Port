@echo off
setlocal EnableDelayedExpansion

:: ============================================================================
:: SMAPI Switch — Windows Build Script
:: Requires:
::   - devkitPro (devkitA64 + libnx) — set DEVKITPRO or install to C:\devkitPro
::   - .NET 8 SDK                     — dotnet.exe must be in PATH
::   - CMake 3.20+                    — cmake.exe must be in PATH
:: ============================================================================

echo ============================================================
echo  SMAPI Switch Build System (Windows)
echo ============================================================
echo.

:: ── Locate devkitPro ─────────────────────────────────────────────────────────
if defined DEVKITPRO goto :dkp_found
if exist "C:\devkitPro\devkitA64" (
    set DEVKITPRO=C:\devkitPro
    goto :dkp_found
)
echo [ERROR] devkitPro not found. Set the DEVKITPRO environment variable or
echo         install devkitPro to C:\devkitPro.
exit /b 1
:dkp_found
echo [INFO] Using devkitPro at: %DEVKITPRO%
set PATH=%DEVKITPRO%\devkitA64\bin;%DEVKITPRO%\tools\bin;%PATH%

:: ── Check .NET SDK ───────────────────────────────────────────────────────────
where dotnet >nul 2>&1
if errorlevel 1 (
    echo [ERROR] dotnet.exe not found. Install the .NET 8 SDK from https://dotnet.microsoft.com/
    exit /b 1
)
echo [INFO] .NET SDK: & dotnet --version

:: ── Check CMake ──────────────────────────────────────────────────────────────
where cmake >nul 2>&1
if errorlevel 1 (
    echo [ERROR] cmake.exe not found. Install CMake 3.20+ and add it to PATH.
    exit /b 1
)
echo [INFO] CMake: & cmake --version | findstr /i "cmake version"

:: ── Create output directories ─────────────────────────────────────────────────
if not exist "build\native"  mkdir "build\native"
if not exist "build\managed" mkdir "build\managed"
if not exist "build\deploy"  mkdir "build\deploy"
echo.

:: ============================================================================
:: Step 1 — Build managed code (.NET)
:: ============================================================================
echo [STEP 1/3] Building managed SMAPI assemblies...
cd /d "%~dp0managed"

dotnet restore SMAPI.Switch.sln
if errorlevel 1 ( echo [ERROR] NuGet restore failed. & exit /b 1 )

dotnet build SMAPI.Switch.sln -c Release -o "..\build\managed"
if errorlevel 1 ( echo [ERROR] Managed build failed. & exit /b 1 )

echo [INFO] Managed build succeeded.
echo.
cd /d "%~dp0"

:: ============================================================================
:: Step 2 — Build native plugin (aarch64-none-elf)
:: ============================================================================
echo [STEP 2/3] Building native sysmodule (aarch64-none-elf)...
cd /d "%~dp0build\native"

cmake ..\.. -G "MinGW Makefiles" ^
    -DCMAKE_TOOLCHAIN_FILE="%~dp0toolchain-switch.cmake" ^
    -DCMAKE_BUILD_TYPE=Release
if errorlevel 1 ( echo [ERROR] CMake configure failed. & exit /b 1 )

cmake --build . --config Release --parallel
if errorlevel 1 ( echo [ERROR] Native build failed. & exit /b 1 )

echo [INFO] Native build succeeded.
echo.
cd /d "%~dp0"

:: ============================================================================
:: Step 3 — Assemble deployment package
:: ============================================================================
echo [STEP 3/3] Assembling SD card deployment package...

set DEPLOY=%~dp0build\deploy
set CONTENTS=%DEPLOY%\atmosphere\contents\0100E65002BB8000

if not exist "%CONTENTS%\exefs"                mkdir "%CONTENTS%\exefs"
if not exist "%CONTENTS%\romfs\smapi-internal" mkdir "%CONTENTS%\romfs\smapi-internal"
if not exist "%CONTENTS%\romfs\Mods"           mkdir "%CONTENTS%\romfs\Mods"
if not exist "%DEPLOY%\switch\smapi\Mods"      mkdir "%DEPLOY%\switch\smapi\Mods"

if exist "%~dp0build\native\smapi_switch.nro" (
    copy /Y "%~dp0build\native\smapi_switch.nro" "%CONTENTS%\exefs\smapi_switch.nro"
    echo [INFO] Copied smapi_switch.nro
) else (
    echo [WARN] smapi_switch.nro not found — native build may have failed silently.
)

xcopy /Y /E /I "%~dp0build\managed\*.dll" "%CONTENTS%\romfs\smapi-internal\"
xcopy /Y /E /I "%~dp0build\managed\*.pdb" "%CONTENTS%\romfs\smapi-internal\"
copy /Y "%~dp0deploy\README.txt" "%DEPLOY%\README.txt" 2>nul

echo.
echo ============================================================
echo  Build complete!  Output in: build\deploy\
echo.
echo  SD card layout:
echo    atmosphere\contents\0100E65002BB8000\exefs\smapi_switch.nro
echo    atmosphere\contents\0100E65002BB8000\romfs\smapi-internal\*.dll
echo    switch\smapi\Mods\     ^<-- put your mods here
echo ============================================================
