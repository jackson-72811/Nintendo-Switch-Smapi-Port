using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace StardewModdingAPI.Framework
{
    /// <summary>
    /// Entry point called by the native sysmodule via Mono's runtime_invoke.
    /// Responsible for:
    ///   1. Initialising logging
    ///   2. Applying Harmony patches to the running game
    ///   3. Loading all mods
    ///   4. Signalling completion
    /// </summary>
    public static class SmapiBootstrap
    {
        private static bool s_initialised = false;
        private static readonly object InitLock = new();

        /// <summary>
        /// Called from native shellcode once the Mono runtime is ready.
        /// Must be public and static so mono_runtime_invoke can find it.
        /// </summary>
        public static int Init() {
            lock (InitLock) {
                if (s_initialised) return 0;
                s_initialised = true;
            }

            // Run init on a background thread so we don't block the injector's
            // remote thread (which returns immediately to the game).
            var t = new Thread(RunInit) {
                IsBackground = true,
                Name = "SMAPI-Switch-Init"
            };
            t.Start();
            return 0;
        }

        private static void RunInit() {
            // Brief yield to allow the game to finish its own initialisation
            Thread.Sleep(2000);

            var coreMonitor = new SmapiMonitor("SMAPI", Constants.LogPath);
            coreMonitor.Log($"SMAPI Switch v{Constants.ApiVersion} starting…", LogLevel.Info);
            coreMonitor.Log($"Mods path: {Constants.ModsPath}", LogLevel.Info);

            try {
                // ── Directory setup ──────────────────────────────────────
                Directory.CreateDirectory(Constants.ModsPath);
                Directory.CreateDirectory(Constants.InternalFilesPath);

                // ── Wait for game to reach a usable state ────────────────
                WaitForGameReady(coreMonitor);

                // ── Build core services ──────────────────────────────────
                var eventManager  = new EventManager(coreMonitor);
                var contentHelper = new SwitchContentHelper(
                    Constants.GamePath, eventManager, coreMonitor);
                var registry      = new ModRegistry();

                // ── Apply Harmony patches ────────────────────────────────
                var hooks = new SwitchGameHooks(eventManager, coreMonitor);
                hooks.ApplyPatches();

                // ── Load mods ────────────────────────────────────────────
                var loader = new ModLoader(Constants.ModsPath, coreMonitor);
                loader.LoadAll(registry, eventManager, contentHelper);

                // ── Fire GameLaunched ────────────────────────────────────
                eventManager.OnGameLaunched();

                coreMonitor.Log("SMAPI initialisation complete.", LogLevel.Info);
            } catch (Exception ex) {
                coreMonitor.Log($"SMAPI initialisation failed: {ex}", LogLevel.Error);
            }
        }

        /// <summary>Spin until StardewValley.Game1 is instantiated.</summary>
        private static void WaitForGameReady(IMonitor monitor) {
            for (int attempt = 0; attempt < 60; attempt++) {
                try {
                    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
                        var t = asm.GetType("StardewValley.Game1");
                        if (t != null) {
                            monitor.Log("StardewValley.Game1 found — proceeding.", LogLevel.Debug);
                            return;
                        }
                    }
                } catch { /* ignore */ }
                Thread.Sleep(1000);
            }
            monitor.Log("Warning: StardewValley.Game1 not found after 60 s — continuing anyway.", LogLevel.Warn);
        }
    }
}
