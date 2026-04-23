using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace StardewModdingAPI.Framework
{
    internal sealed class SwitchTranslationHelper : ITranslationHelper
    {
        private readonly Dictionary<string, Dictionary<string, string>> _data =
            new(StringComparer.OrdinalIgnoreCase);
        private readonly IMonitor _monitor;
        private string _locale = "en";

        public string Locale => _locale;

        public SwitchTranslationHelper(string modDir, IMonitor monitor) {
            _monitor = monitor;
            LoadTranslations(modDir);
        }

        private void LoadTranslations(string modDir) {
            string i18nDir = Path.Combine(modDir, "i18n");
            if (!Directory.Exists(i18nDir)) return;

            foreach (string file in Directory.GetFiles(i18nDir, "*.json")) {
                string locale = Path.GetFileNameWithoutExtension(file).ToLowerInvariant();
                try {
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                        File.ReadAllText(file));
                    if (dict != null)
                        _data[locale] = dict;
                } catch (Exception ex) {
                    _monitor.Log($"Failed to load i18n/{locale}.json: {ex.Message}", LogLevel.Warn);
                }
            }
        }

        public Translation Get(string key) {
            string text = Resolve(key);
            return new Translation(_locale, key, text);
        }

        public Translation Get(string key, object tokens) {
            return Get(key).Tokens(tokens);
        }

        public IEnumerable<string> GetInAllLocales(string key, bool withFallback = false) {
            foreach (var (locale, dict) in _data) {
                if (dict.TryGetValue(key, out string val))
                    yield return val;
            }
        }

        public bool ContainsKey(string key) => Resolve(key) != null;

        internal void SetLocale(string locale) {
            _locale = locale.ToLowerInvariant();
        }

        private string Resolve(string key) {
            // Try exact locale, then base language, then English
            foreach (string candidate in LocaleFallback(_locale)) {
                if (_data.TryGetValue(candidate, out var dict) &&
                    dict.TryGetValue(key, out string val))
                    return val;
            }
            return null;
        }

        private static IEnumerable<string> LocaleFallback(string locale) {
            yield return locale;
            int dash = locale.IndexOf('-');
            if (dash > 0) yield return locale[..dash];
            if (!locale.StartsWith("en")) yield return "en";
        }
    }
}
