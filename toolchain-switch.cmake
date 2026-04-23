set(CMAKE_SYSTEM_NAME Generic)
set(CMAKE_SYSTEM_PROCESSOR aarch64)

# devkitPro root (Windows path — override with DEVKITPRO env var if needed)
if(DEFINED ENV{DEVKITPRO})
    set(DEVKITPRO $ENV{DEVKITPRO})
else()
    set(DEVKITPRO "C:/devkitPro")
endif()

set(DEVKITARM  "${DEVKITPRO}/devkitARM")
set(DEVKITA64  "${DEVKITPRO}/devkitA64")
set(LIBNX      "${DEVKITPRO}/libnx")
set(PORTLIBS   "${DEVKITPRO}/portlibs/switch")

set(CMAKE_C_COMPILER   "${DEVKITA64}/bin/aarch64-none-elf-gcc.exe")
set(CMAKE_CXX_COMPILER "${DEVKITA64}/bin/aarch64-none-elf-g++.exe")
set(CMAKE_AR           "${DEVKITA64}/bin/aarch64-none-elf-ar.exe"     CACHE STRING "")
set(CMAKE_RANLIB       "${DEVKITA64}/bin/aarch64-none-elf-ranlib.exe" CACHE STRING "")
set(CMAKE_STRIP        "${DEVKITA64}/bin/aarch64-none-elf-strip.exe"  CACHE STRING "")
set(ELF2NRO            "${DEVKITPRO}/tools/bin/elf2nro.exe")
set(NACPTOOL           "${DEVKITPRO}/tools/bin/nacptool.exe")

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
