#include "ProcessMonitor.h"
#include "SwitchFS.h"
#include <cstdio>

ProcessMonitor::ProcessMonitor(u64 titleId)
    : m_titleId(titleId), m_pid(0),
      m_debugHandle(INVALID_HANDLE), m_attached(false) {}

ProcessMonitor::~ProcessMonitor() {
    Detach();
}

bool ProcessMonitor::IsTitleRunning() {
    // If already attached, verify the process is still alive.
    if (m_attached) {
        DebugEventInfo evt;
        // Poll for process exit events
        while (R_SUCCEEDED(svcGetDebugEvent(&evt, m_debugHandle))) {
            if (evt.type == DebugEvent_Exception) {
                // Process has died
                if (evt.info.exception.type == ExceptionType_UserBreak) {
                    Detach();
                    return false;
                }
            }
        }
        return true;
    }

    return TryAttach();
}

bool ProcessMonitor::TryAttach() {
    // Get the PID of the running title
    u64 pid = 0;
    Result rc = pmdmntGetApplicationProcessId(&pid);
    if (R_FAILED(rc)) return false;

    // Check if it's our target title
    NsApplicationControlData ctrl = {};
    u64 size = 0;
    // Use nsGetApplicationControlData to get the title ID
    // Alternative: use ldr:dmnt or pm:dmnt to get the title ID
    u64 titleId = 0;
    rc = pminfoGetProgramId(&titleId, pid);
    if (R_FAILED(rc) || titleId != m_titleId) return false;

    // Open a debug handle
    Handle debugHandle = INVALID_HANDLE;
    rc = svcDebugActiveProcess(&debugHandle, pid);
    if (R_FAILED(rc)) {
        char buf[128];
        snprintf(buf, sizeof(buf),
                 "[SMAPI] svcDebugActiveProcess failed: 0x%08X\n", rc);
        SwitchFS::Log(buf);
        return false;
    }

    m_pid         = pid;
    m_debugHandle = debugHandle;
    m_attached    = true;
    return true;
}

void ProcessMonitor::Detach() {
    if (m_debugHandle != INVALID_HANDLE) {
        svcCloseHandle(m_debugHandle);
        m_debugHandle = INVALID_HANDLE;
    }
    m_pid      = 0;
    m_attached = false;
}
