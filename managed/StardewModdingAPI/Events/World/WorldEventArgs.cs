using System;
using System.Collections.Generic;

namespace StardewModdingAPI.Events.World
{
    // Forward-declare game types as stubs — they are resolved at runtime
    // from the game assembly once SMAPI is injected.
    public class GameLocation { }
    public class NPC          { }
    public class Item         { }
    public class Object       { }
    public class TerrainFeature { }
    public class LargeTerrainFeature { }
    public class Building     { }
    public class Debris       { }
    public class Furniture    { }
    public class Chest        { }

    public class LocationListChangedEventArgs : EventArgs
    {
        public IEnumerable<GameLocation> Added   { get; }
        public IEnumerable<GameLocation> Removed { get; }
        internal LocationListChangedEventArgs(IEnumerable<GameLocation> added,
                                               IEnumerable<GameLocation> removed) {
            Added = added; Removed = removed;
        }
    }

    public class BuildingListChangedEventArgs : EventArgs
    {
        public GameLocation             Location { get; }
        public IEnumerable<Building>    Added    { get; }
        public IEnumerable<Building>    Removed  { get; }
        internal BuildingListChangedEventArgs(GameLocation loc,
            IEnumerable<Building> added, IEnumerable<Building> removed) {
            Location = loc; Added = added; Removed = removed;
        }
    }

    public class DebrisListChangedEventArgs : EventArgs
    {
        public GameLocation           Location { get; }
        public IEnumerable<Debris>    Added    { get; }
        public IEnumerable<Debris>    Removed  { get; }
        internal DebrisListChangedEventArgs(GameLocation loc,
            IEnumerable<Debris> added, IEnumerable<Debris> removed) {
            Location = loc; Added = added; Removed = removed;
        }
    }

    public class LargeTerrainFeatureListChangedEventArgs : EventArgs
    {
        public GameLocation                      Location { get; }
        public IEnumerable<LargeTerrainFeature>  Added    { get; }
        public IEnumerable<LargeTerrainFeature>  Removed  { get; }
        internal LargeTerrainFeatureListChangedEventArgs(GameLocation loc,
            IEnumerable<LargeTerrainFeature> added, IEnumerable<LargeTerrainFeature> removed) {
            Location = loc; Added = added; Removed = removed;
        }
    }

    public class NpcListChangedEventArgs : EventArgs
    {
        public GameLocation      Location { get; }
        public IEnumerable<NPC>  Added    { get; }
        public IEnumerable<NPC>  Removed  { get; }
        internal NpcListChangedEventArgs(GameLocation loc,
            IEnumerable<NPC> added, IEnumerable<NPC> removed) {
            Location = loc; Added = added; Removed = removed;
        }
    }

    public class ObjectListChangedEventArgs : EventArgs
    {
        public GameLocation                           Location { get; }
        public IEnumerable<KeyValuePair<object,Object>> Added  { get; }
        public IEnumerable<KeyValuePair<object,Object>> Removed{ get; }
        internal ObjectListChangedEventArgs(GameLocation loc,
            IEnumerable<KeyValuePair<object,Object>> added,
            IEnumerable<KeyValuePair<object,Object>> removed) {
            Location = loc; Added = added; Removed = removed;
        }
    }

    public class ChestInventoryChangedEventArgs : EventArgs
    {
        public Chest              Chest    { get; }
        public GameLocation       Location { get; }
        public IEnumerable<Item>  Added    { get; }
        public IEnumerable<Item>  Removed  { get; }
        public IEnumerable<ItemStackSizeChange> QuantityChanged { get; }
        internal ChestInventoryChangedEventArgs(Chest chest, GameLocation loc,
            IEnumerable<Item> added, IEnumerable<Item> removed,
            IEnumerable<ItemStackSizeChange> quantChanged) {
            Chest = chest; Location = loc;
            Added = added; Removed = removed; QuantityChanged = quantChanged;
        }
    }

    public class ItemStackSizeChange
    {
        public Item Item    { get; }
        public int  OldSize { get; }
        public int  NewSize { get; }
        public ItemStackSizeChange(Item item, int oldSize, int newSize) {
            Item = item; OldSize = oldSize; NewSize = newSize;
        }
    }

    public class TerrainFeatureListChangedEventArgs : EventArgs
    {
        public GameLocation                              Location { get; }
        public IEnumerable<KeyValuePair<object,TerrainFeature>> Added   { get; }
        public IEnumerable<KeyValuePair<object,TerrainFeature>> Removed { get; }
        internal TerrainFeatureListChangedEventArgs(GameLocation loc,
            IEnumerable<KeyValuePair<object,TerrainFeature>> added,
            IEnumerable<KeyValuePair<object,TerrainFeature>> removed) {
            Location = loc; Added = added; Removed = removed;
        }
    }

    public class FurnitureListChangedEventArgs : EventArgs
    {
        public GameLocation           Location { get; }
        public IEnumerable<Furniture> Added    { get; }
        public IEnumerable<Furniture> Removed  { get; }
        internal FurnitureListChangedEventArgs(GameLocation loc,
            IEnumerable<Furniture> added, IEnumerable<Furniture> removed) {
            Location = loc; Added = added; Removed = removed;
        }
    }
}
