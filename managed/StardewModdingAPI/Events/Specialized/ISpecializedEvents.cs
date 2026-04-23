using System;

namespace StardewModdingAPI.Events.Specialized
{
    public interface ISpecializedEvents
    {
        /// <summary>Raised when the game load stage changes (used by content packs during initialisation).</summary>
        event EventHandler<LoadStageChangedEventArgs> LoadStageChanged;

        /// <summary>Raised after the update loop is called (before sleep).</summary>
        event EventHandler<UnvalidatedUpdateTickingEventArgs> UnvalidatedUpdateTicking;

        /// <summary>Raised after the update loop completes.</summary>
        event EventHandler<UnvalidatedUpdateTickedEventArgs> UnvalidatedUpdateTicked;
    }
}
