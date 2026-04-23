using System;
using System.Linq;

namespace StardewModdingAPI.Utilities
{
    public class AssetName : IAssetName, IEquatable<AssetName>
    {
        public string Name       { get; }
        public string LocaleCode { get; }
        public string BaseName   { get; }

        public AssetName(string name, string localeCode = null) {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            // Normalize separators
            name = name.Replace('\\', '/').TrimStart('/');

            LocaleCode = localeCode;
            BaseName   = name;

            Name = localeCode != null ? $"{name}.{localeCode}" : name;
        }

        public static AssetName Parse(string rawName) {
            if (string.IsNullOrWhiteSpace(rawName))
                throw new ArgumentNullException(nameof(rawName));

            rawName = rawName.Replace('\\', '/').TrimStart('/');

            // Detect locale suffix, e.g. "Maps/Town.de"
            int dot = rawName.LastIndexOf('.');
            if (dot > 0 && dot < rawName.Length - 1) {
                string suffix = rawName[(dot + 1)..];
                if (IsLocaleCode(suffix))
                    return new AssetName(rawName[..dot], suffix);
            }
            return new AssetName(rawName);
        }

        public bool IsEquivalentTo(string assetName, bool useBaseName = false) {
            string compare = useBaseName ? BaseName : Name;
            assetName = assetName?.Replace('\\', '/').TrimStart('/');
            return string.Equals(compare, assetName, StringComparison.OrdinalIgnoreCase);
        }

        public bool StartsWith(string prefix, bool allowPartialWord = true, bool allowSubfolder = true) {
            if (prefix == null) return false;
            prefix = prefix.Replace('\\', '/').TrimStart('/');
            if (!BaseName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return false;
            if (BaseName.Length == prefix.Length) return true;
            if (allowSubfolder && BaseName[prefix.Length] == '/') return true;
            if (allowPartialWord) return true;
            return false;
        }

        public bool Equals(AssetName other) =>
            other != null && string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object obj) => obj is AssetName a && Equals(a);
        public override int  GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Name);
        public override string ToString() => Name;

        private static bool IsLocaleCode(string s) =>
            s.Length is 2 or 5 && s.All(c => char.IsLetter(c) || c == '-');
    }
}
