using System;

namespace StardewModdingAPI.Events.Content
{
    public interface IContentEvents
    {
        /// <summary>Raised when the game loads an asset (including from mods).</summary>
        event EventHandler<AssetRequestedEventArgs> AssetRequested;

        /// <summary>Raised after an asset is loaded into the cache.</summary>
        event EventHandler<AssetReadyEventArgs> AssetReady;

        /// <summary>Raised after one or more assets are invalidated from the cache.</summary>
        event EventHandler<AssetsInvalidatedEventArgs> AssetsInvalidated;

        /// <summary>Raised after the locale changes.</summary>
        event EventHandler<LocaleChangedEventArgs> LocaleChanged;
    }
}
