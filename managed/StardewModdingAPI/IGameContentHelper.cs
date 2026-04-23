using System;
using System.Collections.Generic;
using StardewModdingAPI.Utilities;
using Microsoft.Xna.Framework.Graphics;

namespace StardewModdingAPI
{
    /// <summary>
    /// Provides helpers to read, load, and modify the game's content assets
    /// (textures, maps, data files, etc.).
    /// </summary>
    public interface IGameContentHelper
    {
        /// <summary>The current locale code, e.g. "en" or "de".</summary>
        string CurrentLocale { get; }

        /// <summary>Parse a raw asset name into an <see cref="IAssetName"/>.</summary>
        IAssetName ParseAssetName(string rawName);

        /// <summary>
        /// Load a game asset. Mod content editors/loaders will be applied.
        /// </summary>
        T Load<T>(string assetName) where T : class;

        /// <summary>
        /// Load a game asset using a pre-parsed name.
        /// </summary>
        T Load<T>(IAssetName assetName) where T : class;

        /// <summary>
        /// Mark one or more cached assets as stale so they will be reloaded on
        /// next access (triggering <see cref="Events.IContentEvents"/> handlers).
        /// </summary>
        void InvalidateCache(string assetName);

        /// <inheritdoc cref="InvalidateCache(string)"/>
        void InvalidateCache(IAssetName assetName);

        /// <summary>Invalidate all assets that match a predicate.</summary>
        bool InvalidateCache(Func<IAssetName, bool> predicate);

        /// <summary>
        /// Returns true if a game asset exists (after applying mod overrides).
        /// </summary>
        bool DoesAssetExist<T>(IAssetName assetName) where T : class;

        /// <summary>
        /// Get a patch object that lets you edit a loaded asset.
        /// Prefer using <see cref="Events.IContentEvents.AssetRequested"/> instead.
        /// </summary>
        IAssetData GetPatchHelper<T>(T data, string assetName = null) where T : class;
    }

    /// <summary>Encapsulates access and edits for a content asset.</summary>
    public interface IAssetData
    {
        IAssetName Name { get; }
        Type DataType   { get; }
        object Data     { get; }

        IAssetData<TData> GetData<TData>() where TData : class;
    }

    /// <summary>Strongly-typed wrapper for <see cref="IAssetData"/>.</summary>
    public interface IAssetData<TData> : IAssetData where TData : class
    {
        new TData Data { get; set; }

        void ReplaceWith(TData newData);

        void Edit(Action<TData> editor);
    }
}
