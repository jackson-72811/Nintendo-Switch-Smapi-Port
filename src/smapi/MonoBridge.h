#pragma once
#include <switch.h>
#include <cstdint>
#include <vector>
#include <string>

/*
 * MonoBridge — locates the Mono/CoreCLR runtime embedded in the Stardew Valley
 * Switch binary and resolves the API entry points we need for injection.
 *
 * The game ships with an embedded Mono runtime (libmono-2.0 statically linked
 * or as a bundled .nso).  We scan the process' module list for it, then walk
 * its export table to find the symbols we need.
 */
class MonoBridge {
public:
    struct MonoSymbols {
        uintptr_t mono_domain_get;
        uintptr_t mono_domain_assembly_open;
        uintptr_t mono_assembly_get_image;
        uintptr_t mono_class_from_name;
        uintptr_t mono_class_get_method_from_name;
        uintptr_t mono_runtime_invoke;
        uintptr_t mono_thread_attach;
        uintptr_t mono_object_new;
        uintptr_t mono_value_box;
        uintptr_t mono_string_new;
    };

    // Find the base address of the Mono runtime inside the target process.
    static Result FindRuntimeBase(Handle processHandle, uintptr_t& outBase);

    // Resolve all needed Mono API symbols relative to the runtime base.
    static Result ResolveSymbols(Handle processHandle,
                                  uintptr_t runtimeBase,
                                  MonoSymbols& outSymbols);

    // Patch the symbol-pointer slots in a generated shellcode blob.
    static void PatchShellcode(std::vector<uint8_t>& shellcode,
                                const MonoSymbols& syms);

private:
    static Result ScanExportTable(Handle processHandle,
                                   uintptr_t moduleBase,
                                   const std::string& symbolName,
                                   uintptr_t& outAddr);

    static Result FindModuleByName(Handle processHandle,
                                    const std::string& partialName,
                                    uintptr_t& outBase, size_t& outSize);
};
