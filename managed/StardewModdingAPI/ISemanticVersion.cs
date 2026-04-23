namespace StardewModdingAPI
{
    /// <summary>A semantic version with an optional pre-release tag.</summary>
    public interface ISemanticVersion
    {
        int MajorVersion { get; }
        int MinorVersion { get; }
        int PatchVersion { get; }
        string PrereleaseTag { get; }
        string BuildMetadata { get; }

        bool IsPrerelease();
        bool IsNewerThan(ISemanticVersion other);
        bool IsNewerThan(string other);
        bool IsOlderThan(ISemanticVersion other);
        bool IsOlderThan(string other);
        bool IsBetween(ISemanticVersion lowerBound, ISemanticVersion upperBound);
        bool IsBetween(string lowerBound, string upperBound);
        bool IsCompatibleWith(ISemanticVersion apiVersion);
    }
}
