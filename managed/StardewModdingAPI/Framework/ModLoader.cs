using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace StardewModdingAPI.Framework
{
    /// <summary>Scans the Mods directory, loads assemblies, and instantiates mod classes.</summary>
    internal sealed class ModLoader
    {
        private readonly string  _modsPath;
        private readonly IMonitor _coreMonitor;
        private readonly List<LoadedMod> _mods = new();

        public IReadOnlyList<LoadedMod> Mods => _mods;

        public ModLoader(string modsPath, IMonitor coreMonitor) {
            _modsPath    = modsPath;
            _coreMonitor = coreMonitor;
        }

        /// <summary>Scan and load all mods from the Mods directory.</summary>
        public void LoadAll(ModRegistry registry, EventManager eventManager,
                            SwitchContentHelper contentHelper) {
            if (!Directory.Exists(_modsPath)) {
                Directory.CreateDirectory(_modsPath);
                _coreMonitor.Log($"Created empty mods directory at: {_modsPath}", LogLevel.Info);
                return;
            }

            foreach (string modDir in Directory.GetDirectories(_modsPath)) {
                try {
                    TryLoadMod(modDir, registry, eventManager, contentHelper);
                } catch (Exception ex) {
                    _coreMonitor.Log(
                        $"Failed to load mod from '{Path.GetFileName(modDir)}': {ex.Message}",
                        LogLevel.Error);
                }
            }

            _coreMonitor.Log($"Loaded {_mods.Count} mod(s).", LogLevel.Info);
        }

        private void TryLoadMod(string modDir, ModRegistry registry,
                                 EventManager eventManager,
                                 SwitchContentHelper contentHelper) {
            // ── Read manifest ────────────────────────────────────────────
            string manifestPath = Path.Combine(modDir, "manifest.json");
            if (!File.Exists(manifestPath)) {
                _coreMonitor.Log(
                    $"Skipping '{Path.GetFileName(modDir)}': no manifest.json found.",
                    LogLevel.Warn);
                return;
            }

            var manifest = JsonConvert.DeserializeObject<Manifest>(
                File.ReadAllText(manifestPath),
                new JsonSerializerSettings {
                    Converters = { new SemanticVersionConverter() },
                    MissingMemberHandling = MissingMemberHandling.Ignore
                });

            if (manifest == null) {
                _coreMonitor.Log($"Invalid manifest in '{Path.GetFileName(modDir)}'.", LogLevel.Error);
                return;
            }

            // ── Validate manifest ────────────────────────────────────────
            if (string.IsNullOrWhiteSpace(manifest.EntryDll)) {
                // Content-pack style — no assembly, just data files
                _coreMonitor.Log($"[{manifest.Name}] is a content pack (no EntryDll).", LogLevel.Debug);
                return;
            }

            // ── Version check ────────────────────────────────────────────
            if (manifest.MinimumApiVersion != null &&
                manifest.MinimumApiVersion.IsNewerThan(Constants.ApiVersion)) {
                _coreMonitor.Log(
                    $"Skipping '{manifest.Name}': requires SMAPI {manifest.MinimumApiVersion} "
                    + $"(installed: {Constants.ApiVersion}).", LogLevel.Error);
                return;
            }

            // ── Load assembly ────────────────────────────────────────────
            string dllPath = Path.Combine(modDir, manifest.EntryDll);
            if (!File.Exists(dllPath)) {
                _coreMonitor.Log(
                    $"Skipping '{manifest.Name}': EntryDll '{manifest.EntryDll}' not found.",
                    LogLevel.Error);
                return;
            }

            Assembly asm = Assembly.LoadFrom(dllPath);

            // ── Find Mod subclass ────────────────────────────────────────
            Type modType = null;
            foreach (var type in asm.GetTypes()) {
                if (!type.IsAbstract && typeof(Mod).IsAssignableFrom(type)) {
                    modType = type;
                    break;
                }
            }

            if (modType == null) {
                _coreMonitor.Log(
                    $"Skipping '{manifest.Name}': no class inheriting from Mod found.",
                    LogLevel.Error);
                return;
            }

            // ── Instantiate mod ──────────────────────────────────────────
            var modInstance = (Mod)Activator.CreateInstance(modType);

            var monitor  = new SmapiMonitor(manifest.Name, Constants.LogPath);
            var helper   = new ModHelper(modDir, manifest, monitor,
                                         registry, eventManager, contentHelper);

            modInstance.ModManifest = manifest;
            modInstance.Monitor     = monitor;
            modInstance.Helper      = helper;

            var loaded = new LoadedMod(modInstance, manifest, modDir);
            _mods.Add(loaded);
            registry.RegisterMod(loaded);

            _coreMonitor.Log($"Loaded mod: {manifest.Name} {manifest.Version} by {manifest.Author}", LogLevel.Info);

            // ── Call Entry ───────────────────────────────────────────────
            try {
                modInstance.Entry(helper);
            } catch (Exception ex) {
                _coreMonitor.Log($"[{manifest.Name}] Entry() threw: {ex}", LogLevel.Error);
            }
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    internal sealed class LoadedMod
    {
        public Mod      Instance  { get; }
        public IManifest Manifest { get; }
        public string   Directory { get; }

        public LoadedMod(Mod instance, IManifest manifest, string directory) {
            Instance = instance; Manifest = manifest; Directory = directory;
        }
    }

    internal sealed class SemanticVersionConverter : JsonConverter<ISemanticVersion>
    {
        public override ISemanticVersion ReadJson(JsonReader r, Type t, ISemanticVersion ex,
                                                   bool hasExisting, JsonSerializer s) {
            string val = (string)r.Value;
            return val != null ? new SemanticVersion(val) : null;
        }
        public override void WriteJson(JsonWriter w, ISemanticVersion v, JsonSerializer s)
            => w.WriteValue(v?.ToString());
    }
}
