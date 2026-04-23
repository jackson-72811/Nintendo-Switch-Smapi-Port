using System;
using System.Collections.Generic;
using System.IO;

namespace StardewModdingAPI.Framework
{
    /// <summary>Concrete <see cref="IMonitor"/> implementation that writes to a file and console.</summary>
    internal sealed class SmapiMonitor : IMonitor
    {
        private readonly string _source;
        private readonly string _logPath;
        private readonly HashSet<string> _logged = new(StringComparer.Ordinal);
        private static readonly object Lock = new();

        public bool IsVerbose { get; set; } = false;

        internal SmapiMonitor(string source, string logPath) {
            _source  = source;
            _logPath = logPath;
        }

        public void Log(string message, LogLevel level = LogLevel.Trace) {
            WriteToLog(message, level);
        }

        public void VerboseLog(string message) {
            if (IsVerbose) WriteToLog(message, LogLevel.Trace);
        }

        public void LogOnce(string message, LogLevel level = LogLevel.Trace) {
            lock (_logged) {
                if (_logged.Add(message))
                    WriteToLog(message, level);
            }
        }

        private void WriteToLog(string message, LogLevel level) {
            string line = $"[{DateTime.Now:HH:mm:ss} {level,-5} {_source}] {message}";
            lock (Lock) {
                try { File.AppendAllText(_logPath, line + Environment.NewLine); }
                catch { /* Log failures are silently ignored */ }
            }

            // Also write to console if available (useful during emulator testing)
            var color = level switch {
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Warn  => ConsoleColor.Yellow,
                LogLevel.Alert => ConsoleColor.Magenta,
                LogLevel.Info  => ConsoleColor.White,
                _              => ConsoleColor.Gray
            };

            try {
                var orig = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.WriteLine(line);
                Console.ForegroundColor = orig;
            } catch { /* No console available */ }
        }
    }
}
