using System;
using System.Text.RegularExpressions;

namespace StardewModdingAPI
{
    /// <summary>A semantic version (major.minor.patch[-prerelease][+build]).</summary>
    public class SemanticVersion : ISemanticVersion, IComparable<SemanticVersion>, IEquatable<SemanticVersion>
    {
        private static readonly Regex VersionPattern = new(
            @"^(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)"
            + @"(?:-(?<prerelease>[a-zA-Z0-9]+(?:\.[a-zA-Z0-9]+)*))?"
            + @"(?:\+(?<build>[a-zA-Z0-9]+(?:\.[a-zA-Z0-9]+)*))?$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public int    MajorVersion   { get; }
        public int    MinorVersion   { get; }
        public int    PatchVersion   { get; }
        public string PrereleaseTag  { get; }
        public string BuildMetadata  { get; }

        public SemanticVersion(int major, int minor, int patch,
                                string prerelease = null, string build = null) {
            if (major < 0) throw new ArgumentOutOfRangeException(nameof(major));
            if (minor < 0) throw new ArgumentOutOfRangeException(nameof(minor));
            if (patch < 0) throw new ArgumentOutOfRangeException(nameof(patch));
            MajorVersion  = major;
            MinorVersion  = minor;
            PatchVersion  = patch;
            PrereleaseTag = prerelease ?? string.Empty;
            BuildMetadata = build ?? string.Empty;
        }

        public SemanticVersion(string version) {
            if (version == null) throw new ArgumentNullException(nameof(version));
            var m = VersionPattern.Match(version.Trim());
            if (!m.Success)
                throw new FormatException($"The value '{version}' is not a valid semantic version.");
            MajorVersion  = int.Parse(m.Groups["major"].Value);
            MinorVersion  = int.Parse(m.Groups["minor"].Value);
            PatchVersion  = int.Parse(m.Groups["patch"].Value);
            PrereleaseTag = m.Groups["prerelease"].Value;
            BuildMetadata = m.Groups["build"].Value;
        }

        public bool IsPrerelease() => !string.IsNullOrEmpty(PrereleaseTag);

        public bool IsNewerThan(ISemanticVersion other) => CompareTo(other) > 0;
        public bool IsNewerThan(string other) => IsNewerThan(new SemanticVersion(other));
        public bool IsOlderThan(ISemanticVersion other) => CompareTo(other) < 0;
        public bool IsOlderThan(string other) => IsOlderThan(new SemanticVersion(other));

        public bool IsBetween(ISemanticVersion lower, ISemanticVersion upper)
            => CompareTo(lower) >= 0 && CompareTo(upper) <= 0;

        public bool IsBetween(string lower, string upper)
            => IsBetween(new SemanticVersion(lower), new SemanticVersion(upper));

        public bool IsCompatibleWith(ISemanticVersion apiVersion) {
            if (apiVersion == null) return true;
            // Compatible if same major version and not newer than the API
            return MajorVersion == apiVersion.MajorVersion && !IsNewerThan(apiVersion);
        }

        public int CompareTo(SemanticVersion other) => CompareTo((ISemanticVersion)other);

        public int CompareTo(ISemanticVersion other) {
            if (other == null) return 1;
            int c = MajorVersion.CompareTo(other.MajorVersion);
            if (c != 0) return c;
            c = MinorVersion.CompareTo(other.MinorVersion);
            if (c != 0) return c;
            c = PatchVersion.CompareTo(other.PatchVersion);
            if (c != 0) return c;

            bool thisPrerelease  = IsPrerelease();
            bool otherPrerelease = !string.IsNullOrEmpty(other.PrereleaseTag);
            if (!thisPrerelease && otherPrerelease) return 1;
            if (thisPrerelease && !otherPrerelease) return -1;
            return string.Compare(PrereleaseTag, other.PrereleaseTag,
                                  StringComparison.OrdinalIgnoreCase);
        }

        public bool Equals(SemanticVersion other) => CompareTo(other) == 0;
        public override bool Equals(object obj) => obj is SemanticVersion v && Equals(v);
        public override int GetHashCode() =>
            HashCode.Combine(MajorVersion, MinorVersion, PatchVersion, PrereleaseTag);

        public override string ToString() {
            string s = $"{MajorVersion}.{MinorVersion}.{PatchVersion}";
            if (IsPrerelease()) s += $"-{PrereleaseTag}";
            if (!string.IsNullOrEmpty(BuildMetadata)) s += $"+{BuildMetadata}";
            return s;
        }

        public static bool operator ==(SemanticVersion a, SemanticVersion b) =>
            a?.CompareTo(b) == 0;
        public static bool operator !=(SemanticVersion a, SemanticVersion b) => !(a == b);
        public static bool operator  <(SemanticVersion a, SemanticVersion b) =>
            a?.CompareTo(b) < 0;
        public static bool operator  >(SemanticVersion a, SemanticVersion b) =>
            a?.CompareTo(b) > 0;

        public static implicit operator SemanticVersion(string version) =>
            version != null ? new SemanticVersion(version) : null;
    }
}
