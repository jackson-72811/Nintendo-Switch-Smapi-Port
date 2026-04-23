using System;
using System.IO;

namespace StardewModdingAPI
{
    /// <summary>Contains SMAPI's constants and pseudo-constants.</summary>
    public static class Constants
    {
        // ── Version ──────────────────────────────────────────────────────────
        public static ISemanticVersion ApiVersion { get; } = new SemanticVersion(4, 1, 0);

        public static string MinimumGameVersion { get; } = "1.6.0";
        public static string MaximumGameVersion { get; } = null;

        // ── Platform ─────────────────────────────────────────────────────────
        public static GamePlatform TargetPlatform { get; } = GamePlatform.NintendoSwitch;

        // ── Paths ────────────────────────────────────────────────────────────

        /// <summary>Root of the SMAPI data folder on the SD card.</summary>
        public static string SmapiRootPath { get; } = GetSmapiRoot();

        /// <summary>The folder that contains all installed mods.</summary>
        public static string ModsPath { get; } = Path.Combine(SmapiRootPath, "Mods");

        /// <summary>The folder containing SMAPI's internal data files.</summary>
        public static string InternalFilesPath { get; } = Path.Combine(SmapiRootPath, "smapi-internal");

        /// <summary>Path to the SMAPI log file.</summary>
        public static string LogPath { get; } = Path.Combine(SmapiRootPath, "SMAPI-latest.txt");

        /// <summary>Path to the game's content folder (exposed via Atmosphere LayeredFS).</summary>
        public static string ContentPath { get; } = Path.Combine(GamePath, "Content");

        /// <summary>The game's base directory.</summary>
        public static string GamePath { get; } = GetGamePath();

        // ── Execution context ─────────────────────────────────────────────────
        public static bool IsDedicatedServer { get; } = false;
        public static bool IsMultiplayer     { get; internal set; } = false;

        // ── Helpers ──────────────────────────────────────────────────────────

        private static string GetSmapiRoot() {
            // On Switch the SD card is mounted at /switch/smapi by our sysmodule
            string sdPath = "/switch/smapi";
            if (Directory.Exists(sdPath)) return sdPath;
            // Fallback for build/testing on Windows
            return Path.Combine(AppContext.BaseDirectory, "smapi-data");
        }

        private static string GetGamePath() {
            // On Switch the game's romfs is exposed at this path by Atmosphere
            string romfsPath = "/atmosphere/contents/0100E65002BB8000/romfs";
            if (Directory.Exists(romfsPath)) return romfsPath;
            return AppContext.BaseDirectory;
        }
    }

    /// <summary>The game's operating platform.</summary>
    public enum GamePlatform
    {
        Windows,
        Linux,
        Mac,
        Android,
        NintendoSwitch
    }
}
