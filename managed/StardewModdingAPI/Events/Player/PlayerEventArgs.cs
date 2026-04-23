using System;
using System.Collections.Generic;
using StardewModdingAPI.Events.World;

namespace StardewModdingAPI.Events.Player
{
    // Forward-declare game Farmer type
    public class Farmer { }

    public class InventoryChangedEventArgs : EventArgs
    {
        public Farmer            Player          { get; }
        public IEnumerable<Item> Added           { get; }
        public IEnumerable<Item> Removed         { get; }
        public IEnumerable<ItemStackSizeChange> QuantityChanged { get; }
        public bool              IsLocalPlayer   { get; }

        internal InventoryChangedEventArgs(Farmer player,
            IEnumerable<Item> added, IEnumerable<Item> removed,
            IEnumerable<ItemStackSizeChange> quantChanged, bool isLocal) {
            Player = player; Added = added; Removed = removed;
            QuantityChanged = quantChanged; IsLocalPlayer = isLocal;
        }
    }

    public class LevelChangedEventArgs : EventArgs
    {
        public Farmer  Player        { get; }
        public string  Skill         { get; }
        public int     OldLevel      { get; }
        public int     NewLevel      { get; }
        public bool    IsLocalPlayer { get; }

        internal LevelChangedEventArgs(Farmer player, string skill,
                                        int oldLevel, int newLevel, bool isLocal) {
            Player = player; Skill = skill;
            OldLevel = oldLevel; NewLevel = newLevel; IsLocalPlayer = isLocal;
        }
    }

    public class WarpedEventArgs : EventArgs
    {
        public Farmer       Player      { get; }
        public GameLocation OldLocation { get; }
        public GameLocation NewLocation { get; }
        public bool         IsLocalPlayer { get; }

        internal WarpedEventArgs(Farmer player,
            GameLocation oldLoc, GameLocation newLoc, bool isLocal) {
            Player = player; OldLocation = oldLoc; NewLocation = newLoc;
            IsLocalPlayer = isLocal;
        }
    }
}
