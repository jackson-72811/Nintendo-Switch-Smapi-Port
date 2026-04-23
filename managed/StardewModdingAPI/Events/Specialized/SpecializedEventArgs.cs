using System;

namespace StardewModdingAPI.Events.Specialized
{
    public enum LoadStage
    {
        None,
        ReturningToTitle,
        SaveParsed,
        SaveLoadedBasicInfo,
        Loaded,
        CreatedInitialLocations,
        PostLoadContent
    }

    public class LoadStageChangedEventArgs : EventArgs
    {
        public LoadStage OldStage { get; }
        public LoadStage NewStage { get; }
        internal LoadStageChangedEventArgs(LoadStage oldStage, LoadStage newStage) {
            OldStage = oldStage; NewStage = newStage;
        }
    }

    public class UnvalidatedUpdateTickingEventArgs : EventArgs
    {
        public uint Ticks { get; }
        internal UnvalidatedUpdateTickingEventArgs(uint ticks) { Ticks = ticks; }
    }

    public class UnvalidatedUpdateTickedEventArgs : EventArgs
    {
        public uint Ticks { get; }
        internal UnvalidatedUpdateTickedEventArgs(uint ticks) { Ticks = ticks; }
    }
}
