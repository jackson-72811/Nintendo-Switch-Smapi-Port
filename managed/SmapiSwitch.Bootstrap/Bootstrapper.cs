using System;
using StardewModdingAPI.Framework;

namespace SmapiSwitch.Bootstrap
{
    /// <summary>
    /// Thin public wrapper around <see cref="SmapiBootstrap.Init"/>.
    ///
    /// The native sysmodule calls this class via:
    ///   MonoClass  *klass  = mono_class_from_name(image, "SmapiSwitch.Bootstrap", "Bootstrapper");
    ///   MonoMethod *method = mono_class_get_method_from_name(klass, "Init", 0);
    ///   mono_runtime_invoke(method, NULL, NULL, NULL);
    /// </summary>
    public static class Bootstrapper
    {
        /// <summary>
        /// Called by the native injector.  Must be public, static, and
        /// parameter-free so mono_runtime_invoke can call it without boxing.
        /// </summary>
        public static void Init() {
            try {
                SmapiBootstrap.Init();
            } catch (Exception ex) {
                // Last-resort fallback: write to a known log path
                try {
                    System.IO.File.AppendAllText(
                        "/switch/smapi/bootstrap-error.log",
                        $"[{DateTime.Now:s}] Bootstrap.Init threw: {ex}\n");
                } catch { /* truly unrecoverable */ }
            }
        }
    }
}
