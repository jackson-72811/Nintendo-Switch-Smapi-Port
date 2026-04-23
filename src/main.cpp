/*
 * SMAPI Switch — native sysmodule entry point
 *
 * Compiled with devkitPro (aarch64-none-elf-g++) and linked against libnx.
 * Runs as a background sysmodule under Atmosphere CFW; waits for the Stardew
 * Valley title (0100E65002BB8000) to start, then injects the managed SMAPI
 * bootstrap into its .NET / Mono runtime.
 */

#include <switch.h>
#include <cstdio>
#include <cstring>
#include <thread>
#include <chrono>

#include "smapi/SmapiPlugin.h"
#include "platform/ProcessMonitor.h"
#include "platform/SwitchFS.h"

// Stardew Valley title ID on Nintendo Switch
static constexpr u64 SDV_TITLE_ID = 0x0100E65002BB8000ULL;

// libnx sysmodule heap (512 KiB is sufficient for our housekeeping code;
// the heavy lifting is done inside the game's own address space).
extern "C" u32 __nx_applet_type = AppletType_None;
extern "C" u64 __nx_heap_size   = 0x80000; // 512 KiB

static void svcSleepMs(u64 ms) {
    svcSleepThread(ms * 1000000ULL);
}

int main(int, char**) {
    // ── System initialisation ─────────────────────────────────────────────
    smGetCurrentProc(); // anchor our sysmodule handle

    SwitchFS::Init();

    SmapiPlugin plugin;
    ProcessMonitor monitor(SDV_TITLE_ID);

    SwitchFS::Log("[SMAPI-Switch] Sysmodule started — waiting for Stardew Valley...\n");

    // ── Main watch loop ───────────────────────────────────────────────────
    while (appletMainLoop()) {
        if (monitor.IsTitleRunning()) {
            if (!plugin.IsInjected()) {
                SwitchFS::Log("[SMAPI-Switch] Stardew Valley detected — injecting SMAPI...\n");
                Result rc = plugin.Inject(monitor.GetProcessHandle());
                if (R_SUCCEEDED(rc)) {
                    SwitchFS::Log("[SMAPI-Switch] Injection successful.\n");
                } else {
                    char buf[128];
                    snprintf(buf, sizeof(buf),
                             "[SMAPI-Switch] Injection failed: 0x%08X\n", rc);
                    SwitchFS::Log(buf);
                }
            }
        } else {
            if (plugin.IsInjected()) {
                SwitchFS::Log("[SMAPI-Switch] Stardew Valley exited — resetting state.\n");
                plugin.Reset();
            }
        }
        svcSleepMs(500);
    }

    SwitchFS::Cleanup();
    return 0;
}
