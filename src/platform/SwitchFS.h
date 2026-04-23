#pragma once
#include <string>
#include <vector>

/*
 * Thin wrapper around Switch filesystem services (fsdev / romfsdev).
 * Used by the sysmodule for logging and reading the mods directory on the SD card.
 */
class SwitchFS {
public:
    static void Init();
    static void Cleanup();

    // Append a line to the SMAPI log on the SD card.
    static void Log(const char* msg);
    static void LogFmt(const char* fmt, ...);

    // Check whether a path exists on the SD card.
    static bool Exists(const std::string& path);

    // List directory entries (file/dir names only).
    static std::vector<std::string> ListDirectory(const std::string& path);

    // Read entire file into a byte buffer; returns false on failure.
    static bool ReadFile(const std::string& path, std::vector<uint8_t>& out);

    // Write bytes to a file, creating it if necessary.
    static bool WriteFile(const std::string& path,
                          const void* data, size_t size);

    // Create a directory (and all parents), silently succeeds if already exists.
    static bool CreateDirs(const std::string& path);

private:
    static bool s_initialised;
};
