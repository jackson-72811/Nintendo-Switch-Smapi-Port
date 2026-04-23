using System;
using System.IO;
using Newtonsoft.Json;
using StardewModdingAPI;

namespace StardewModdingAPI.Framework
{
    internal sealed class SwitchDataHelper : IDataHelper
    {
        private readonly string   _modDir;
        private readonly IManifest _manifest;
        private readonly IMonitor  _monitor;

        private static string SaveDataRoot =>
            Path.Combine(Constants.SmapiRootPath, "save-data");

        public SwitchDataHelper(string modDir, IManifest manifest, IMonitor monitor) {
            _modDir   = modDir;
            _manifest = manifest;
            _monitor  = monitor;
        }

        // ── Per-save data ─────────────────────────────────────────────────────

        public TData ReadSaveData<TData>(string key) where TData : class {
            string path = GetSaveDataPath(key);
            return ReadJson<TData>(path);
        }

        public void WriteSaveData<TData>(string key, TData data) where TData : class
            => WriteJson(GetSaveDataPath(key), data);

        // ── Global data ───────────────────────────────────────────────────────

        public TData ReadGlobalData<TData>(string key) where TData : class {
            string path = GetGlobalDataPath(key);
            return ReadJson<TData>(path);
        }

        public void WriteGlobalData<TData>(string key, TData data) where TData : class
            => WriteJson(GetGlobalDataPath(key), data);

        // ── JSON files ────────────────────────────────────────────────────────

        public TModel ReadJsonFile<TModel>(string path) where TModel : class
            => ReadJson<TModel>(Path.Combine(_modDir, path));

        public void WriteJsonFile<TModel>(string path, TModel data) where TModel : class, new()
            => WriteJson(Path.Combine(_modDir, path), data);

        // ── Helpers ──────────────────────────────────────────────────────────

        private string GetSaveDataPath(string key) {
            string saveFolder = GetCurrentSaveFolderName() ?? "unknown";
            return Path.Combine(SaveDataRoot, _manifest.UniqueID, saveFolder, $"{key}.json");
        }

        private string GetGlobalDataPath(string key)
            => Path.Combine(SaveDataRoot, _manifest.UniqueID, "global", $"{key}.json");

        private static string GetCurrentSaveFolderName() {
            try {
                var game1Type = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectType("StardewValley.Game1");
                if (game1Type == null) return null;
                var prop = game1Type.GetField("saveFileName",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                return prop?.GetValue(null) as string;
            } catch { return null; }
        }

        private TModel ReadJson<TModel>(string path) where TModel : class {
            if (!File.Exists(path)) return null;
            try {
                return JsonConvert.DeserializeObject<TModel>(File.ReadAllText(path));
            } catch (Exception ex) {
                _monitor.Log($"Failed to read data file '{path}': {ex.Message}", LogLevel.Warn);
                return null;
            }
        }

        private void WriteJson<TModel>(string path, TModel data) where TModel : class {
            try {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllText(path, JsonConvert.SerializeObject(data, Formatting.Indented));
            } catch (Exception ex) {
                _monitor.Log($"Failed to write data file '{path}': {ex.Message}", LogLevel.Warn);
            }
        }
    }
}
