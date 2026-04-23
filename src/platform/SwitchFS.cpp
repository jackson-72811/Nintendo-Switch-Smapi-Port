#include "SwitchFS.h"
#include <switch.h>
#include <cstdio>
#include <cstring>
#include <cstdarg>
#include <dirent.h>
#include <sys/stat.h>

bool SwitchFS::s_initialised = false;

static const char* kLogPath = "sdmc:/switch/smapi/smapi-switch.log";
static FILE* s_logFile = nullptr;

void SwitchFS::Init() {
    if (s_initialised) return;
    fsdevMountSdmc();
    // Ensure log directory exists
    mkdir("sdmc:/switch",        0777);
    mkdir("sdmc:/switch/smapi",  0777);
    s_logFile = fopen(kLogPath, "a");
    s_initialised = true;
}

void SwitchFS::Cleanup() {
    if (s_logFile) { fclose(s_logFile); s_logFile = nullptr; }
    fsdevUnmountAll();
    s_initialised = false;
}

void SwitchFS::Log(const char* msg) {
    if (s_logFile) {
        fputs(msg, s_logFile);
        fflush(s_logFile);
    }
}

void SwitchFS::LogFmt(const char* fmt, ...) {
    if (!s_logFile) return;
    va_list args;
    va_start(args, fmt);
    vfprintf(s_logFile, fmt, args);
    va_end(args);
    fflush(s_logFile);
}

bool SwitchFS::Exists(const std::string& path) {
    struct stat st;
    return stat(path.c_str(), &st) == 0;
}

std::vector<std::string> SwitchFS::ListDirectory(const std::string& path) {
    std::vector<std::string> result;
    DIR* dir = opendir(path.c_str());
    if (!dir) return result;
    struct dirent* entry;
    while ((entry = readdir(dir)) != nullptr) {
        if (entry->d_name[0] == '.') continue;
        result.push_back(entry->d_name);
    }
    closedir(dir);
    return result;
}

bool SwitchFS::ReadFile(const std::string& path, std::vector<uint8_t>& out) {
    FILE* f = fopen(path.c_str(), "rb");
    if (!f) return false;
    fseek(f, 0, SEEK_END);
    long sz = ftell(f);
    fseek(f, 0, SEEK_SET);
    if (sz <= 0) { fclose(f); return false; }
    out.resize((size_t)sz);
    bool ok = (fread(out.data(), 1, sz, f) == (size_t)sz);
    fclose(f);
    return ok;
}

bool SwitchFS::WriteFile(const std::string& path,
                          const void* data, size_t size) {
    FILE* f = fopen(path.c_str(), "wb");
    if (!f) return false;
    bool ok = (fwrite(data, 1, size, f) == size);
    fclose(f);
    return ok;
}

bool SwitchFS::CreateDirs(const std::string& path) {
    // Walk the path, creating each component
    std::string cur;
    for (char c : path) {
        cur += c;
        if (c == '/' && cur.size() > 1) {
            mkdir(cur.c_str(), 0777); // ignore errors (may already exist)
        }
    }
    return true;
}
