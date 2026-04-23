namespace StardewModdingAPI
{
    /// <summary>The base interface for an SMAPI mod.</summary>
    public interface IMod
    {
        IModHelper Helper     { get; }
        IMonitor   Monitor    { get; }
        IManifest  ModManifest{ get; }
    }
}
