namespace StardewModdingAPI
{
    /// <summary>Provides helpers to read/write per-save and global data files.</summary>
    public interface IDataHelper
    {
        // ── Per-save data (stored alongside the save file) ──────────────────

        /// <summary>Read per-save data for the current save slot.</summary>
        TData ReadSaveData<TData>(string key) where TData : class;

        /// <summary>Write per-save data for the current save slot.</summary>
        void WriteSaveData<TData>(string key, TData data) where TData : class;

        // ── Global data (stored in the mod's folder on the SD card) ──────────

        /// <summary>Read global data that persists across save files.</summary>
        TData ReadGlobalData<TData>(string key) where TData : class;

        /// <summary>Write global data that persists across save files.</summary>
        void WriteGlobalData<TData>(string key, TData data) where TData : class;

        // ── JSON helpers ─────────────────────────────────────────────────────

        /// <summary>Read a JSON file from the mod's folder.</summary>
        TModel ReadJsonFile<TModel>(string path) where TModel : class;

        /// <summary>Write a JSON file to the mod's folder.</summary>
        void WriteJsonFile<TModel>(string path, TModel data) where TModel : class, new();
    }
}
