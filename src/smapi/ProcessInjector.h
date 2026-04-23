#pragma once
#include <vector>
#include <cstdint>

/*
 * Builds position-independent AArch64 shellcode that, when executed inside
 * the target process, loads the managed SMAPI bootstrap DLL via the Mono API
 * functions already present in the game's address space.
 *
 * Shellcode calling convention (matches mono internal ABI):
 *   x0 = pointer to NUL-terminated assembly path string
 *   Returns in x0: 0 on success, non-zero on error.
 */
class ProcessInjector {
public:
    // Size reserved for the generated shellcode (must be <= actual generated size).
    static size_t ShellcodeSize();

    // Build the shellcode blob.
    // pathAddr — virtual address (inside the target process) of the path string.
    static std::vector<uint8_t> BuildMonoLoadShellcode(uintptr_t pathAddr);

private:
    // AArch64 helpers
    static uint32_t EncodeMOVZ(int reg, uint16_t imm, int shift);
    static uint32_t EncodeMOVK(int reg, uint16_t imm, int shift);
    static uint32_t EncodeBLR(int reg);
    static uint32_t EncodeRET();
    static uint32_t EncodeMOV(int dst, int src);
    static uint32_t EncodeSTR(int src, int base, int offset);
    static uint32_t EncodeLDR(int dst, int base, int offset);
    static uint32_t EncodeADD(int dst, int src, int imm12);

    static void EmitMoveImm64(std::vector<uint32_t>& insns, int reg, uint64_t val);
};
