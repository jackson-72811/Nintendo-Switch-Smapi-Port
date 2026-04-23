#include "SmapiPlugin.h"
#include "ProcessInjector.h"
#include "MonoBridge.h"
#include "../platform/SwitchFS.h"
#include <cstring>
#include <cstdio>

SmapiPlugin::SmapiPlugin()
    : m_injected(false), m_processHandle(INVALID_HANDLE) {}

SmapiPlugin::~SmapiPlugin() {
    Reset();
}

Result SmapiPlugin::Inject(Handle processHandle) {
    if (m_injected) return 0;
    m_processHandle = processHandle;

    // Step 1 — locate Mono/CoreCLR runtime base in the target process
    uintptr_t monoBase = 0;
    Result rc = FindMonoRuntime(processHandle, monoBase);
    if (R_FAILED(rc)) {
        SwitchFS::Log("[SMAPI] Could not locate Mono runtime in game process.\n");
        return rc;
    }

    char buf[128];
    snprintf(buf, sizeof(buf), "[SMAPI] Mono runtime base: 0x%016lX\n", monoBase);
    SwitchFS::Log(buf);

    // Step 2 — inject the bootstrap shellcode + managed assembly path
    rc = InjectMonoBootstrap(processHandle);
    if (R_FAILED(rc)) {
        SwitchFS::Log("[SMAPI] Bootstrap injection failed.\n");
        return rc;
    }

    m_injected = true;
    return 0;
}

void SmapiPlugin::Reset() {
    if (m_processHandle != INVALID_HANDLE) {
        svcCloseHandle(m_processHandle);
        m_processHandle = INVALID_HANDLE;
    }
    m_injected = false;
}

// ── Private helpers ──────────────────────────────────────────────────────────

Result SmapiPlugin::InjectMonoBootstrap(Handle processHandle) {
    // The bootstrap DLL path on the SD card (accessible via Atmosphere LayeredFS)
    static const char kBootstrapPath[] =
        "sdmc:/atmosphere/contents/0100E65002BB8000/romfs/smapi-internal/SmapiSwitch.Bootstrap.dll";

    // Allocate remote memory for the path string
    uintptr_t remotePathAddr = 0;
    size_t pathLen = strlen(kBootstrapPath) + 1;

    Result rc = AllocateRemoteMemory(processHandle,
                                     pathLen + ProcessInjector::ShellcodeSize(),
                                     remotePathAddr);
    if (R_FAILED(rc)) return rc;

    // Write the path string
    rc = WriteRemoteMemory(processHandle, remotePathAddr,
                           kBootstrapPath, pathLen);
    if (R_FAILED(rc)) return rc;

    // Build and write the shellcode that calls mono_domain_assembly_open +
    // mono_assembly_get_image + mono_runtime_invoke("Bootstrap", "Init")
    uintptr_t shellcodeAddr = remotePathAddr + pathLen;
    auto shellcode = ProcessInjector::BuildMonoLoadShellcode(remotePathAddr);

    rc = WriteRemoteMemory(processHandle, shellcodeAddr,
                           shellcode.data(), shellcode.size());
    if (R_FAILED(rc)) return rc;

    // Mark shellcode region as RX
    rc = svcSetProcessMemoryPermission(processHandle, shellcodeAddr,
                                       shellcode.size(), Perm_Rx);
    if (R_FAILED(rc)) return rc;

    // Kick off a remote thread at the shellcode entry
    return CreateRemoteThread(processHandle, shellcodeAddr, remotePathAddr);
}

Result SmapiPlugin::FindMonoRuntime(Handle processHandle, uintptr_t& outBase) {
    return MonoBridge::FindRuntimeBase(processHandle, outBase);
}

Result SmapiPlugin::AllocateRemoteMemory(Handle processHandle,
                                          size_t size, uintptr_t& outAddr) {
    // Round up to page boundary
    size = (size + 0xFFF) & ~0xFFFULL;

    // Walk the address space to find a free region
    MemoryInfo memInfo;
    u32 pageInfo;
    uintptr_t addr = 0x1000000; // start past NULL guard

    while (addr < 0x7FFFFFFFULL) {
        Result rc = svcQueryProcessMemory(&memInfo, &pageInfo, processHandle, addr);
        if (R_FAILED(rc)) break;

        if (memInfo.type == MemType_Unmapped && memInfo.size >= size) {
            outAddr = memInfo.addr;
            rc = svcSetProcessMemoryPermission(processHandle, outAddr, size, Perm_Rw);
            return rc;
        }
        addr = memInfo.addr + memInfo.size;
        if (addr <= memInfo.addr) break; // overflow guard
    }
    return MAKERESULT(Module_Kernel, KernelError_OutOfMemory);
}

Result SmapiPlugin::WriteRemoteMemory(Handle processHandle,
                                       uintptr_t dest,
                                       const void* src, size_t size) {
    u64 written = 0;
    return svcWriteDebugProcessMemory(processHandle, src, dest, size);
}

Result SmapiPlugin::CreateRemoteThread(Handle processHandle,
                                        uintptr_t entry, uintptr_t arg) {
    Handle threadHandle;
    Result rc = svcCreateThread(&threadHandle, (ThreadFunc)(void*)entry,
                                (void*)arg, nullptr, 32, -2);
    if (R_FAILED(rc)) return rc;
    rc = svcStartThread(threadHandle);
    svcCloseHandle(threadHandle);
    return rc;
}
