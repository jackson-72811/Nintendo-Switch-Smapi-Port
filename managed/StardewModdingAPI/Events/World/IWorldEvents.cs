using System;

namespace StardewModdingAPI.Events.World
{
    public interface IWorldEvents
    {
        event EventHandler<LocationListChangedEventArgs>       LocationListChanged;
        event EventHandler<BuildingListChangedEventArgs>       BuildingListChanged;
        event EventHandler<DebrisListChangedEventArgs>         DebrisListChanged;
        event EventHandler<LargeTerrainFeatureListChangedEventArgs> LargeTerrainFeatureListChanged;
        event EventHandler<NpcListChangedEventArgs>            NpcListChanged;
        event EventHandler<ObjectListChangedEventArgs>         ObjectListChanged;
        event EventHandler<ChestInventoryChangedEventArgs>     ChestInventoryChanged;
        event EventHandler<TerrainFeatureListChangedEventArgs> TerrainFeatureListChanged;
        event EventHandler<FurnitureListChangedEventArgs>      FurnitureListChanged;
    }
}
