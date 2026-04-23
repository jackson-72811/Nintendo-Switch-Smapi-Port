using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StardewModdingAPI
{
    /// <summary>A translated string with token-substitution support.</summary>
    public class Translation
    {
        private readonly string _locale;
        private readonly string _key;
        private          string _text;

        public static readonly Translation NotFound = new("", "", null);

        public bool HasValue() => _text != null;

        internal Translation(string locale, string key, string text) {
            _locale = locale;
            _key    = key;
            _text   = text;
        }

        /// <summary>Substitute {{TokenName}} placeholders with values from an anonymous object.</summary>
        public Translation Tokens(object tokens) {
            if (_text == null || tokens == null) return this;
            var props = tokens.GetType().GetProperties();
            string result = _text;
            foreach (var p in props) {
                string placeholder = "{{" + p.Name + "}}";
                result = result.Replace(placeholder, p.GetValue(tokens)?.ToString() ?? "", StringComparison.OrdinalIgnoreCase);
            }
            return new Translation(_locale, _key, result);
        }

        /// <summary>Substitute {{TokenName}} placeholders from a dictionary.</summary>
        public Translation Tokens(IDictionary<string, object> tokens) {
            if (_text == null || tokens == null) return this;
            string result = _text;
            foreach (var kv in tokens)
                result = result.Replace("{{" + kv.Key + "}}", kv.Value?.ToString() ?? "", StringComparison.OrdinalIgnoreCase);
            return new Translation(_locale, _key, result);
        }

        public override string ToString() => _text ?? $"(no translation:{_locale}:{_key})";

        public static implicit operator string(Translation t) => t?.ToString();
    }
}
