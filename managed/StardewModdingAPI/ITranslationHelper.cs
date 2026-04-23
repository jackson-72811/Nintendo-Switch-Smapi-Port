using System.Collections.Generic;

namespace StardewModdingAPI
{
    /// <summary>Provides helpers for reading the mod's i18n translations.</summary>
    public interface ITranslationHelper
    {
        /// <summary>The current locale code (e.g. "en", "de", "zh-CN").</summary>
        string Locale { get; }

        /// <summary>Get a translation for the current locale.</summary>
        Translation Get(string key);

        /// <summary>Get a translation for the current locale with token substitution.</summary>
        Translation Get(string key, object tokens);

        /// <summary>Get all translations for the current locale.</summary>
        IEnumerable<string> GetInAllLocales(string key, bool withFallback = false);

        /// <summary>Returns true if a translation exists for the given key.</summary>
        bool ContainsKey(string key);
    }
}
