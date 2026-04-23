using System;

namespace StardewModdingAPI.Events.GameLoop
{
    /// <summary>Events linked to the game's main update loop.</summary>
    public interface IGameLoopEvents
    {
        /// <summary>Raised once, after the game is fully loaded and all mods have been initialized.</summary>
        event EventHandler<GameLaunchedEventArgs> GameLaunched;

        /// <summary>Raised before the game updates its state each tick.</summary>
        event EventHandler<UpdateTickingEventArgs> UpdateTicking;

        /// <summary>Raised after the game updates its state each tick.</summary>
        event EventHandler<UpdateTickedEventArgs> UpdateTicked;

        /// <summary>Raised once per second, before the game updates.</summary>
        event EventHandler<OneSecondUpdateTickingEventArgs> OneSecondUpdateTicking;

        /// <summary>Raised once per second, after the game updates.</summary>
        event EventHandler<OneSecondUpdateTickedEventArgs> OneSecondUpdateTicked;

        /// <summary>Raised before the game creates a new save slot.</summary>
        event EventHandler<SaveCreatingEventArgs> SaveCreating;

        /// <summary>Raised after the game creates a new save slot.</summary>
        event EventHandler<SaveCreatedEventArgs> SaveCreated;

        /// <summary>Raised before the game serialises save data.</summary>
        event EventHandler<SavingEventArgs> Saving;

        /// <summary>Raised after the game serialises save data.</summary>
        event EventHandler<SavedEventArgs> Saved;

        /// <summary>Raised after the player loads a save slot.</summary>
        event EventHandler<SaveLoadedEventArgs> SaveLoaded;

        /// <summary>Raised after the in-game clock changes.</summary>
        event EventHandler<TimeChangedEventArgs> TimeChanged;

        /// <summary>Raised after a new day begins.</summary>
        event EventHandler<DayStartedEventArgs> DayStarted;

        /// <summary>Raised before the day ends.</summary>
        event EventHandler<DayEndingEventArgs> DayEnding;

        /// <summary>Raised after the player returns to the title screen.</summary>
        event EventHandler<ReturnedToTitleEventArgs> ReturnedToTitle;
    }
}
