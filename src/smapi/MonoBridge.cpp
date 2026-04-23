#include "MonoBridge.h"
#include "../platform/SwitchFS.h"
#include <cstring>
#include <cstdio>

// ── ELF / NSO helpers ─────────────────────────────────────────────────────────

// Minimal ELF64 structures needed to walk the dynamic symbol table
struct Elf64_Sym {
    uint32_t st_name;
    uint8_t  st_info;
    uint8_t  st_other;
    uint16_t st_shndx;
    uint64_t st_value;
    uint64_t st_size;
};

struct Elf64_Dyn {
    int64_t  d_tag;
    uint64_t d_val; // or d_ptr
};

enum {
    DT_NULL    = 0,
    DT_SYMTAB  = 6,
    DT_STRTAB  = 5,
    DT_STRSZ   = 10,
    DT_HASH    = 4,
    DT_GNU_HASH = 0x6ffffef5,
    DT_SYMENT  = 11,
};

static Result ReadRemote(Handle proc, uintptr_t addr, void* buf, size_t len) {
    u64 read = 0;
    return svcReadDebugProcessMemory(buf, proc, addr, len);
}

// ── Public API ────────────────────────────────────────────────────────────────

Result MonoBridge::FindRuntimeBase(Handle processHandle, uintptr_t& outBase) {
    // Strategy:
    // 1. Walk the process memory map looking for a readable+executable region
    //    whose first 4 bytes match the NSO0 magic or ELF magic.
    // 2. For each candidate, attempt to resolve "mono_domain_get" from its
    //    symbol table; success means we found the runtime module.
    MemoryInfo memInfo;
    u32 pageInfo;
    uintptr_t addr = 0x08000000; // typical code region start on Switch

    while (addr < 0x7FC0000000ULL) {
        Result rc = svcQueryProcessMemory(&memInfo, &pageInfo, processHandle, addr);
        if (R_FAILED(rc)) break;

        if ((memInfo.perm & Perm_Rx) && memInfo.size >= 0x10000) {
            // Read first 8 bytes to check magic
            uint8_t magic[8] = {};
            if (R_SUCCEEDED(ReadRemote(processHandle, memInfo.addr, magic, sizeof(magic)))) {
                // NSO0 magic: "NSO0"
                // ELF magic: 0x7F 'E' 'L' 'F'
                bool isNso = (memcmp(magic, "NSO0", 4) == 0);
                bool isElf = (magic[0] == 0x7F && magic[1] == 'E');

                if (isNso || isElf) {
                    uintptr_t probe = 0;
                    Result symbolRc = ScanExportTable(processHandle,
                                                      memInfo.addr,
                                                      "mono_domain_get",
                                                      probe);
                    if (R_SUCCEEDED(symbolRc) && probe != 0) {
                        outBase = memInfo.addr;
                        return 0;
                    }
                }
            }
        }

        uintptr_t nextAddr = memInfo.addr + memInfo.size;
        if (nextAddr <= addr) break;
        addr = nextAddr;
    }

    return MAKERESULT(Module_Kernel, KernelError_InvalidMemState);
}

Result MonoBridge::ResolveSymbols(Handle processHandle,
                                    uintptr_t runtimeBase,
                                    MonoSymbols& outSymbols) {
    struct { const char* name; uintptr_t* target; } syms[] = {
        { "mono_domain_get",                  &outSymbols.mono_domain_get },
        { "mono_domain_assembly_open",         &outSymbols.mono_domain_assembly_open },
        { "mono_assembly_get_image",           &outSymbols.mono_assembly_get_image },
        { "mono_class_from_name",              &outSymbols.mono_class_from_name },
        { "mono_class_get_method_from_name",   &outSymbols.mono_class_get_method_from_name },
        { "mono_runtime_invoke",               &outSymbols.mono_runtime_invoke },
        { "mono_thread_attach",                &outSymbols.mono_thread_attach },
        { "mono_object_new",                   &outSymbols.mono_object_new },
        { "mono_value_box",                    &outSymbols.mono_value_box },
        { "mono_string_new",                   &outSymbols.mono_string_new },
    };

    for (auto& s : syms) {
        Result rc = ScanExportTable(processHandle, runtimeBase,
                                    s.name, *s.target);
        if (R_FAILED(rc) || *s.target == 0) {
            char buf[128];
            snprintf(buf, sizeof(buf),
                     "[SMAPI] WARNING: Could not resolve %s\n", s.name);
            SwitchFS::Log(buf);
        }
    }
    return 0;
}

void MonoBridge::PatchShellcode(std::vector<uint8_t>& shellcode,
                                  const MonoSymbols& syms) {
    // The shellcode has 6 × 8-byte pointer slots starting at byte offset 8
    // (after the prologue STP/MOV instructions).
    auto patch = [&](size_t slotIndex, uintptr_t value) {
        size_t offset = 8 + slotIndex * 8;
        if (offset + 8 <= shellcode.size())
            memcpy(shellcode.data() + offset, &value, 8);
    };

    patch(0, syms.mono_domain_get);
    patch(1, syms.mono_domain_assembly_open);
    patch(2, syms.mono_assembly_get_image);
    patch(3, syms.mono_class_from_name);
    patch(4, syms.mono_class_get_method_from_name);
    patch(5, syms.mono_runtime_invoke);
}

// ── Private helpers ───────────────────────────────────────────────────────────

Result MonoBridge::ScanExportTable(Handle processHandle,
                                    uintptr_t moduleBase,
                                    const std::string& symbolName,
                                    uintptr_t& outAddr) {
    outAddr = 0;

    // Read ELF header to find DYNAMIC segment
    uint8_t elfHeader[64] = {};
    Result rc = ReadRemote(processHandle, moduleBase, elfHeader, sizeof(elfHeader));
    if (R_FAILED(rc)) return rc;

    // ELF64: e_phoff at offset 32 (8 bytes), e_phentsize at 54, e_phnum at 56
    uint64_t phOff  = 0;
    uint16_t phSize = 0, phNum = 0;
    memcpy(&phOff,  elfHeader + 32, 8);
    memcpy(&phSize, elfHeader + 54, 2);
    memcpy(&phNum,  elfHeader + 56, 2);

    if (phSize == 0 || phNum > 256) return MAKERESULT(1, 1);

    uintptr_t dynAddr = 0;
    for (uint16_t i = 0; i < phNum; i++) {
        uint8_t phdr[56] = {};
        rc = ReadRemote(processHandle, moduleBase + phOff + i * phSize, phdr, sizeof(phdr));
        if (R_FAILED(rc)) continue;

        uint32_t pType = 0; memcpy(&pType, phdr, 4);
        if (pType == 2 /*PT_DYNAMIC*/) {
            uint64_t pVAddr = 0; memcpy(&pVAddr, phdr + 16, 8);
            dynAddr = moduleBase + pVAddr;
            break;
        }
    }

    if (dynAddr == 0) return MAKERESULT(1, 2);

    // Walk DYNAMIC segment to find DT_SYMTAB, DT_STRTAB, DT_HASH
    uintptr_t symtab = 0, strtab = 0;
    uint64_t  strsz  = 0;
    uint32_t  nbuckets = 0, nchains = 0;
    uintptr_t hashAddr = 0;

    for (int i = 0; i < 128; i++) {
        Elf64_Dyn dyn = {};
        rc = ReadRemote(processHandle, dynAddr + i * sizeof(Elf64_Dyn),
                        &dyn, sizeof(dyn));
        if (R_FAILED(rc)) break;
        if (dyn.d_tag == DT_NULL) break;

        switch (dyn.d_tag) {
            case DT_SYMTAB:  symtab   = moduleBase + dyn.d_val; break;
            case DT_STRTAB:  strtab   = moduleBase + dyn.d_val; break;
            case DT_STRSZ:   strsz    = dyn.d_val;              break;
            case DT_HASH:    hashAddr = moduleBase + dyn.d_val; break;
        }
    }

    if (symtab == 0 || strtab == 0) return MAKERESULT(1, 3);

    // Read nbuckets/nchains from ELF hash table
    uint32_t hashHeader[2] = {};
    rc = ReadRemote(processHandle, hashAddr, hashHeader, sizeof(hashHeader));
    if (R_FAILED(rc)) return rc;
    nbuckets = hashHeader[0];
    nchains  = hashHeader[1];

    // Linear scan of the symbol table (chains from the hash table)
    for (uint32_t si = 0; si < nchains; si++) {
        Elf64_Sym sym = {};
        rc = ReadRemote(processHandle, symtab + si * sizeof(Elf64_Sym),
                        &sym, sizeof(sym));
        if (R_FAILED(rc)) continue;
        if (sym.st_name == 0 || sym.st_value == 0) continue;

        // Read the name
        char name[256] = {};
        uint32_t nameOff = sym.st_name;
        if (nameOff >= strsz) continue;
        ReadRemote(processHandle, strtab + nameOff, name, sizeof(name) - 1);

        if (symbolName == name) {
            outAddr = moduleBase + sym.st_value;
            return 0;
        }
    }

    return MAKERESULT(1, 4); // not found
}
