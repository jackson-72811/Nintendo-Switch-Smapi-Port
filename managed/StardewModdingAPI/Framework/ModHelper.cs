using System;
using System.IO;
using Newtonsoft.Json;
using StardewModdingAPI.Events;

namespace StardewModdingAPI.Framework
{
    internal sealed class ModHelper : IModHelper
    {
        private readonly IManifest          _manifest;
        private readonly IMonitor           _monitor;
        private readonly ModRegistry        _registry;
        private readonly EventManager       _events;
        private readonly SwitchContentHelper _content;

        public string            DirectoryPath  { get; }
        public IModEvents        Events         => _events;
        public IDataHelper       Data           { get; }
        public IModContentHelper ModContent     { get; }
        public IGameContentHelper GameContent   => _content;
        public IInputHelper      Input          { get; }
        public IMultiplayerHelper Multiplayer   { get; }
        public IReflectionHelper Reflection     { get; }
        public IModRegistry      ModRegistry    => _registry;
        public ICommandHelper    ConsoleCommands{ get; }
        public ITranslationHelper Translation   { get; }

        public ModHelper(string directoryPath, IManifest manifest, IMonitor monitor,
                          ModRegistry registry, EventManager events,
                          SwitchContentHelper content) {
            DirectoryPath  = directoryPath;
            _manifest      = manifest;
            _monitor       = monitor;
            _registry      = registry;
            _events        = events;
            _content       = content;

            Data            = new SwitchDataHelper(directoryPath, manifest, monitor);
            ModContent      = new SwitchModContentHelper(directoryPath, manifest);
            Input           = new SwitchInputHelper();
            Multiplayer     = new SwitchMultiplayerHelper(monitor);
            Reflection      = new SwitchReflectionHelper(monitor);
            ConsoleCommands = new SwitchCommandHelper(manifest.UniqueID, monitor);
            Translation     = new SwitchTranslationHelper(directoryPath, monitor);
        }

        public TConfig ReadConfig<TConfig>() where TConfig : class, new() {
            string path = Path.Combine(DirectoryPath, "config.json");
            if (!File.Exists(path)) {
                var def = new TConfig();
                WriteConfig(def);
                return def;
            }
            try {
                return JsonConvert.DeserializeObject<TConfig>(File.ReadAllText(path))
                       ?? new TConfig();
            } catch (Exception ex) {
                _monitor.Log($"Failed to read config.json: {ex.Message}", LogLevel.Warn);
                return new TConfig();
            }
        }

        public void WriteConfig<TConfig>(TConfig config) where TConfig : class, new() {
            string path = Path.Combine(DirectoryPath, "config.json");
            try {
                File.WriteAllText(path,
                    JsonConvert.SerializeObject(config, Formatting.Indented));
            } catch (Exception ex) {
                _monitor.Log($"Failed to write config.json: {ex.Message}", LogLevel.Warn);
            }
        }
    }
}
