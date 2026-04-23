using System;
using StardewModdingAPI.Events;

namespace StardewModdingAPI
{
    /// <summary>Provides simplified API access for mods.</summary>
    public interface IModHelper
    {
        /// <summary>The absolute path to the mod's folder.</summary>
        string DirectoryPath { get; }

        /// <summary>Exposes events raised by SMAPI and the game.</summary>
        IModEvents Events { get; }

        /// <summary>Reads and writes persistent mod data.</summary>
        IDataHelper Data { get; }

        /// <summary>Reads and writes the mod config file.</summary>
        IModContentHelper ModContent { get; }

        /// <summary>Reads and edits the game's content assets.</summary>
        IGameContentHelper GameContent { get; }

        /// <summary>Gets the current input state.</summary>
        IInputHelper Input { get; }

        /// <summary>Provides multiplayer utilities.</summary>
        IMultiplayerHelper Multiplayer { get; }

        /// <summary>Simplifies access to private game code.</summary>
        IReflectionHelper Reflection { get; }

        /// <summary>Lists and retrieves other mods.</summary>
        IModRegistry ModRegistry { get; }

        /// <summary>Registers and handles SMAPI console commands.</summary>
        ICommandHelper ConsoleCommands { get; }

        /// <summary>Gets translated text for the mod's i18n strings.</summary>
        ITranslationHelper Translation { get; }

        // ── Config ──────────────────────────────────────────────────────────

        /// <summary>Read the mod's config.json, creating it with defaults if absent.</summary>
        TConfig ReadConfig<TConfig>() where TConfig : class, new();

        /// <summary>Save the mod's config.json.</summary>
        void WriteConfig<TConfig>(TConfig config) where TConfig : class, new();
    }
}
