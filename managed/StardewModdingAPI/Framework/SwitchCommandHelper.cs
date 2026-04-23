using System;
using System.Collections.Generic;

namespace StardewModdingAPI.Framework
{
    internal sealed class SwitchCommandHelper : ICommandHelper
    {
        private readonly string  _modID;
        private readonly IMonitor _monitor;
        private static readonly Dictionary<string, (string Doc, Action<string, string[]> Callback)>
            Commands = new(StringComparer.OrdinalIgnoreCase);

        public SwitchCommandHelper(string modID, IMonitor monitor) {
            _modID   = modID;
            _monitor = monitor;
        }

        public void Add(string name, string documentation,
                         Action<string, string[]> callback) {
            if (Commands.ContainsKey(name))
                _monitor.Log($"Command '{name}' is already registered — overwriting.", LogLevel.Warn);
            Commands[name] = (documentation, callback);
        }

        public bool Trigger(string name, string[] args) {
            if (!Commands.TryGetValue(name, out var cmd)) return false;
            try {
                cmd.Callback(name, args);
                return true;
            } catch (Exception ex) {
                _monitor.Log($"Command '{name}' threw: {ex}", LogLevel.Error);
                return false;
            }
        }

        // ── Static helpers used by the SMAPI console ─────────────────────────

        internal static bool ExecuteCommand(string input) {
            if (string.IsNullOrWhiteSpace(input)) return false;
            var parts = input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string name = parts[0];
            string[] args = parts.Length > 1 ? parts[1..] : Array.Empty<string>();

            if (!Commands.TryGetValue(name, out var cmd)) return false;
            cmd.Callback(name, args);
            return true;
        }

        internal static IEnumerable<(string Name, string Doc)> GetAllCommands() {
            foreach (var kv in Commands)
                yield return (kv.Key, kv.Value.Doc);
        }
    }
}
