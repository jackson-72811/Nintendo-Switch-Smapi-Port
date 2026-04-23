namespace StardewModdingAPI.Utilities
{
    /// <summary>An asset name as used by SMAPI's content pipeline.</summary>
    public interface IAssetName
    {
        /// <summary>The normalized asset name, e.g. "Maps/spring_outdoorsTileSheet".</summary>
        string Name { get; }

        /// <summary>The locale code appended to the name, or null.</summary>
        string LocaleCode { get; }

        /// <summary>The base name without locale suffix.</summary>
        string BaseName { get; }

        /// <summary>Whether this asset name is equivalent to another.</summary>
        bool IsEquivalentTo(string assetName, bool useBaseName = false);

        /// <summary>Whether this asset name starts with a given prefix.</summary>
        bool StartsWith(string prefix, bool allowPartialWord = true, bool allowSubfolder = true);
    }
}
