cmake_minimum_required(VERSION 3.20)
project(SmapiSwitch VERSION 1.0.0 LANGUAGES C CXX)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# ── Include paths ────────────────────────────────────────────────────────────
if(DEFINED ENV{DEVKITPRO})
    set(DEVKITPRO $ENV{DEVKITPRO})
else()
    set(DEVKITPRO "C:/devkitPro")
endif()
set(LIBNX    "${DEVKITPRO}/libnx")
set(PORTLIBS "${DEVKITPRO}/portlibs/switch")

include_directories(
    ${LIBNX}/include
    ${PORTLIBS}/include
    ${CMAKE_SOURCE_DIR}/src
)

# ── Source files ─────────────────────────────────────────────────────────────
set(NATIVE_SOURCES
    src/main.cpp
    src/smapi/SmapiPlugin.cpp
    src/smapi/ProcessInjector.cpp
    src/smapi/MonoBridge.cpp
    src/platform/SwitchFS.cpp
    src/platform/ProcessMonitor.cpp
)

add_executable(smapi_switch.elf ${NATIVE_SOURCES})

target_link_libraries(smapi_switch.elf
    ${DEVKITPRO}/libnx/lib/libnx.a
    m c
)

target_compile_options(smapi_switch.elf PRIVATE
    -Wall -Wextra -O2
    -ffunction-sections -fdata-sections
)

target_link_options(smapi_switch.elf PRIVATE
    -Wl,--gc-sections
    -specs=${LIBNX}/switch.specs
)

# ── Post-build: produce .nro ─────────────────────────────────────────────────
set(ELF2NRO "${DEVKITPRO}/tools/bin/elf2nro.exe")
set(NACPTOOL "${DEVKITPRO}/tools/bin/nacptool.exe")
set(NACP_FILE "${CMAKE_BINARY_DIR}/smapi_switch.nacp")

add_custom_command(TARGET smapi_switch.elf POST_BUILD
    COMMAND "${NACPTOOL}" --create
        "SMAPI Switch" "Concerned Ape / SMAPI Team" "1.0.0"
        "${NACP_FILE}"
    COMMAND "${ELF2NRO}"
        "$<TARGET_FILE:smapi_switch.elf>"
        "${CMAKE_BINARY_DIR}/smapi_switch.nro"
        "--nacp=${NACP_FILE}"
        "--icon=${CMAKE_SOURCE_DIR}/assets/icon.jpg"
    COMMENT "Generating smapi_switch.nro"
)
