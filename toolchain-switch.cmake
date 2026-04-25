set(CMAKE_SYSTEM_NAME Generic)
set(CMAKE_SYSTEM_PROCESSOR aarch64)

# ── Locate devkitPro ─────────────────────────────────────────────────────────
if(DEFINED ENV{DEVKITPRO})
    set(DEVKITPRO $ENV{DEVKITPRO})
elseif(CMAKE_HOST_SYSTEM_NAME STREQUAL "Windows")
    set(DEVKITPRO "C:/devkitPro")
else()
    set(DEVKITPRO "/opt/devkitpro")
endif()

# Tool extension: .exe on Windows, empty on Linux/macOS
if(CMAKE_HOST_SYSTEM_NAME STREQUAL "Windows")
    set(EXE ".exe")
else()
    set(EXE "")
endif()

set(DEVKITA64  "${DEVKITPRO}/devkitA64")
set(LIBNX      "${DEVKITPRO}/libnx")
set(PORTLIBS   "${DEVKITPRO}/portlibs/switch")

set(CMAKE_C_COMPILER   "${DEVKITA64}/bin/aarch64-none-elf-gcc${EXE}")
set(CMAKE_CXX_COMPILER "${DEVKITA64}/bin/aarch64-none-elf-g++${EXE}")
set(CMAKE_AR           "${DEVKITA64}/bin/aarch64-none-elf-ar${EXE}"     CACHE STRING "")
set(CMAKE_RANLIB       "${DEVKITA64}/bin/aarch64-none-elf-ranlib${EXE}" CACHE STRING "")
set(CMAKE_STRIP        "${DEVKITA64}/bin/aarch64-none-elf-strip${EXE}"  CACHE STRING "")
set(ELF2NRO            "${DEVKITPRO}/tools/bin/elf2nro${EXE}")
set(NACPTOOL           "${DEVKITPRO}/tools/bin/nacptool${EXE}")

set(CMAKE_FIND_ROOT_PATH "${DEVKITA64}" "${LIBNX}" "${PORTLIBS}")
set(CMAKE_FIND_ROOT_PATH_MODE_PROGRAM NEVER)
set(CMAKE_FIND_ROOT_PATH_MODE_LIBRARY ONLY)
set(CMAKE_FIND_ROOT_PATH_MODE_INCLUDE ONLY)

set(NX_ARCH_SETTINGS "-march=armv8-a+crc+crypto -mtune=cortex-a57 -mtp=soft -fPIC -ftls-model=local-exec")
set(NX_LIB_DIRS      "-L${LIBNX}/lib -L${PORTLIBS}/lib")
set(NX_INCLUDE_DIRS  "${LIBNX}/include;${PORTLIBS}/include")

set(CMAKE_C_FLAGS_INIT   "${NX_ARCH_SETTINGS} -D__SWITCH__")
set(CMAKE_CXX_FLAGS_INIT "${NX_ARCH_SETTINGS} -D__SWITCH__ -std=c++20")
set(CMAKE_EXE_LINKER_FLAGS_INIT "${NX_LIB_DIRS} -specs=${LIBNX}/switch.specs")

set(NX_LIBRARIES "-lnx -lm -lc")
