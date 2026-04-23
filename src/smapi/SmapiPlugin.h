#pragma once
#include <switch.h>

class SmapiPlugin {
public:
    SmapiPlugin();
    ~SmapiPlugin();

    // Inject the managed SMAPI bootstrap into the running game process.
    Result Inject(Handle processHandle);

    // True once injection has been performed for the current game session.
    bool IsInjected() const { return m_injected; }

    // Reset state after the game process terminates.
    void Reset();

private:
    bool   m_injected;
    Handle m_processHandle;

    Result InjectMonoBootstrap(Handle processHandle);
    Result FindMonoRuntime(Handle processHandle, uintptr_t& outBase);
    Result AllocateRemoteMemory(Handle processHandle, size_t size, uintptr_t& outAddr);
    Result WriteRemoteMemory(Handle processHandle, uintptr_t dest, const void* src, size_t size);
    Result CreateRemoteThread(Handle processHandle, uintptr_t entry, uintptr_t arg);
};
