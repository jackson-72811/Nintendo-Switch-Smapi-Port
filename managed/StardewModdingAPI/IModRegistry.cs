using System.Collections.Generic;

namespace StardewModdingAPI
{
    /// <summary>Provides helpers for fetching metadata about loaded mods.</summary>
    public interface IModRegistry
    {
        /// <summary>Get metadata for a mod by its unique ID.</summary>
        IModInfo Get(string uniqueID);

        /// <summary>Get metadata for all loaded mods.</summary>
        IEnumerable<IModInfo> GetAll();

        /// <summary>Returns true if the given mod is loaded.</summary>
        bool IsLoaded(string uniqueID);

        /// <summary>
        /// Get the API surface exposed by a mod, cast to the given interface.
        /// Returns null if the mod is not loaded or doesn't expose that type.
        /// </summary>
        TApi GetApi<TApi>(string uniqueID) where TApi : class;
    }

    /// <summary>Metadata about a loaded mod.</summary>
    public interface IModInfo
    {
        IManifest Manifest  { get; }
        bool      IsContentPack { get; }
        IContentPack AsContentPack() ;
    }

    public interface IContentPack
    {
        string           DirectoryPath { get; }
        IManifest        Manifest      { get; }
        IGameContentHelper GameContent { get; }
        IModContentHelper  ModContent  { get; }
        ITranslationHelper Translation { get; }

        T       ReadJsonFile<T>(string path) where T : class;
        void    WriteJsonFile<T>(string path, T data) where T : class, new();
        bool    HasFile(string path);
    }
}
