#pragma once
#include <switch.h>
#include <cstdint>

/*
 * Watches for a specific title to be running on the system and provides
 * a debug handle to its process so we can inspect / modify its memory.
 */
class ProcessMonitor {
public:
    explicit ProcessMonitor(u64 titleId);
    ~ProcessMonitor();

    // Returns true if the target title is currently running.
    bool IsTitleRunning();

    // Returns a debug handle to the running process (valid only while
    // IsTitleRunning() is true).  Caller does NOT own the handle.
    Handle GetProcessHandle() const { return m_debugHandle; }

private:
    u64    m_titleId;
    u64    m_pid;
    Handle m_debugHandle;
    bool   m_attached;

    bool TryAttach();
    void Detach();
};
