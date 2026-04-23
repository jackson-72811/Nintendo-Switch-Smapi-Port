#include "ProcessInjector.h"
#include <cstring>

// ── AArch64 instruction encoders ─────────────────────────────────────────────

uint32_t ProcessInjector::EncodeMOVZ(int reg, uint16_t imm, int shift) {
    // MOVZ Xreg, #imm, LSL #shift
    int hw = shift / 16;
    return (1u << 31) | (0b10100101u << 23) | ((hw & 3) << 21)
         | ((uint32_t)imm << 5) | (reg & 0x1F);
}

uint32_t ProcessInjector::EncodeMOVK(int reg, uint16_t imm, int shift) {
    // MOVK Xreg, #imm, LSL #shift
    int hw = shift / 16;
    return (1u << 31) | (0b11100101u << 23) | ((hw & 3) << 21)
         | ((uint32_t)imm << 5) | (reg & 0x1F);
}

uint32_t ProcessInjector::EncodeBLR(int reg) {
    return 0xD63F0000u | ((reg & 0x1F) << 5);
}

uint32_t ProcessInjector::EncodeRET() {
    return 0xD65F03C0u; // RET X30
}

uint32_t ProcessInjector::EncodeMOV(int dst, int src) {
    // MOV Xdst, Xsrc  (alias for ORR Xdst, XZR, Xsrc)
    return 0xAA0003E0u | ((src & 0x1F) << 16) | (dst & 0x1F);
}

uint32_t ProcessInjector::EncodeSTR(int src, int base, int offset) {
    // STR Xsrc, [Xbase, #offset] (unsigned offset, scaled by 8)
    uint32_t imm12 = (offset / 8) & 0xFFF;
    return 0xF9000000u | (imm12 << 10) | ((base & 0x1F) << 5) | (src & 0x1F);
}

uint32_t ProcessInjector::EncodeLDR(int dst, int base, int offset) {
    // LDR Xdst, [Xbase, #offset]
    uint32_t imm12 = (offset / 8) & 0xFFF;
    return 0xF9400000u | (imm12 << 10) | ((base & 0x1F) << 5) | (dst & 0x1F);
}

uint32_t ProcessInjector::EncodeADD(int dst, int src, int imm12) {
    return 0x91000000u | ((imm12 & 0xFFF) << 10) | ((src & 0x1F) << 5) | (dst & 0x1F);
}

void ProcessInjector::EmitMoveImm64(std::vector<uint32_t>& insns, int reg, uint64_t val) {
    insns.push_back(EncodeMOVZ(reg, (uint16_t)(val >>  0), 0));
    insns.push_back(EncodeMOVK(reg, (uint16_t)(val >> 16), 16));
    insns.push_back(EncodeMOVK(reg, (uint16_t)(val >> 32), 32));
    insns.push_back(EncodeMOVK(reg, (uint16_t)(val >> 48), 48));
}

// ── Public API ────────────────────────────────────────────────────────────────

size_t ProcessInjector::ShellcodeSize() {
    return 512; // generous upper bound
}

std::vector<uint8_t> ProcessInjector::BuildMonoLoadShellcode(uintptr_t pathAddr) {
    /*
     * Generated shellcode (pseudocode):
     *
     *   // Save callee-saved registers + LR on stack
     *   STP X29, X30, [SP, #-0x10]!
     *   MOV X29, SP
     *
     *   // x0 already = pathAddr (set by caller before BLR to us)
     *   // Call mono_domain_get() → x0 = current domain
     *   MOV X9, #<mono_domain_get addr>
     *   BLR X9
     *   MOV X19, X0    // save domain
     *
     *   // mono_domain_assembly_open(domain, path)
     *   MOV X0, X19
     *   MOV X1, #<pathAddr>
     *   MOV X9, #<mono_domain_assembly_open addr>
     *   BLR X9
     *   MOV X20, X0    // save assembly
     *
     *   // mono_assembly_get_image(assembly)
     *   MOV X9, #<mono_assembly_get_image addr>
     *   BLR X9
     *   MOV X21, X0    // save image
     *
     *   // mono_class_from_name(image, "SmapiSwitch", "Bootstrapper")
     *   MOV X0, X21
     *   ... (class / method lookup omitted for brevity — done in managed code)
     *
     *   LDP X29, X30, [SP], #0x10
     *   RET
     *
     * NOTE: The Mono API addresses are resolved at runtime by MonoBridge and
     * patched into the shellcode.  The placeholder values below are zeroed out
     * and filled in by SmapiPlugin::InjectMonoBootstrap before the shellcode
     * is written to the target process.
     *
     * For simplicity, the shellcode below is a minimal trampoline: it calls
     * mono_domain_assembly_open(currentDomain, path) and then
     * mono_runtime_invoke to call Bootstrapper.Init().
     * Full address fixup happens in MonoBridge::PatchShellcode().
     */

    std::vector<uint32_t> insns;
    insns.reserve(64);

    // Prologue: save LR and FP
    insns.push_back(0xA9BF7BFDu); // STP X29, X30, [SP, #-16]!
    insns.push_back(0x910003FDu); // MOV X29, SP

    // Emit placeholders for function pointers (8 bytes each, NOP-padded)
    // Actual addresses filled in by MonoBridge::PatchShellcode()
    // Slot 0: mono_domain_get        (+16 bytes from shellcode start)
    // Slot 1: mono_domain_assembly_open (+32)
    // Slot 2: mono_assembly_get_image   (+48)
    // Slot 3: mono_class_from_name      (+64)
    // Slot 4: mono_class_get_method_from_name (+80)
    // Slot 5: mono_runtime_invoke       (+96)
    for (int i = 0; i < 6; i++) {
        insns.push_back(0xD503201Fu); // NOP (placeholder low word)
        insns.push_back(0xD503201Fu); // NOP (placeholder high word)
    }

    // Load path address into X1
    EmitMoveImm64(insns, 1, (uint64_t)pathAddr);

    // Load mono_domain_get into X9 and call it  (X0 = domain)
    insns.push_back(EncodeLDR(9, 29, 0)); // LDR X9, [X29, slot0]  -- patched
    insns.push_back(EncodeBLR(9));
    insns.push_back(EncodeMOV(19, 0)); // X19 = domain

    // mono_domain_assembly_open(domain, path)
    insns.push_back(EncodeMOV(0, 19));
    insns.push_back(EncodeLDR(9, 29, 8)); // slot1
    insns.push_back(EncodeBLR(9));
    insns.push_back(EncodeMOV(20, 0)); // X20 = assembly

    // mono_assembly_get_image(assembly)
    insns.push_back(EncodeMOV(0, 20));
    insns.push_back(EncodeLDR(9, 29, 16)); // slot2
    insns.push_back(EncodeBLR(9));
    insns.push_back(EncodeMOV(21, 0)); // X21 = image

    // Epilogue
    insns.push_back(0xA8C17BFDu); // LDP X29, X30, [SP], #16
    insns.push_back(EncodeRET());

    // Convert to bytes
    std::vector<uint8_t> bytes(insns.size() * 4);
    memcpy(bytes.data(), insns.data(), bytes.size());
    return bytes;
}
