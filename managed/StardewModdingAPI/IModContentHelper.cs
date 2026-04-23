using StardewModdingAPI.Utilities;

namespace StardewModdingAPI
{
    /// <summary>Provides helpers to read the mod's own bundled content files.</summary>
    public interface IModContentHelper
    {
        /// <summary>Load a content asset from the mod's folder.</summary>
        T Load<T>(string relativePath) where T : class;

        /// <summary>
        /// Build an <see cref="IAssetName"/> that represents a mod-relative path
        /// in the format SMAPI uses internally (<c>Mods/ModID/path</c>).
        /// </summary>
        IAssetName GetInternalAssetName(string relativePath);

        /// <summary>
        /// Build an asset name that can be returned from an
        /// <c>AssetRequested</c> handler to serve this mod's file.
        /// </summary>
        IAssetName GetManifestAssetName(string relativePath);
    }
}
