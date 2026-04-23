namespace StardewModdingAPI
{
    /// <summary>
    /// The base class for an SMAPI mod.  All mods must inherit from this.
    /// </summary>
    public abstract class Mod : IMod
    {
        /// <summary>Helper APIs for reading/writing mod data.</summary>
        public IModHelper Helper { get; internal set; }

        /// <summary>Writes messages to the console and log file.</summary>
        public IMonitor Monitor { get; internal set; }

        /// <summary>The mod's manifest data.</summary>
        public IManifest ModManifest { get; internal set; }

        /// <summary>Called once after the mod is first loaded.</summary>
        public abstract void Entry(IModHelper helper);

        /// <summary>
        /// Returns an object that exposes an API other mods can consume.
        /// Return <c>null</c> if the mod doesn't expose an API.
        /// </summary>
        public virtual object GetApi() => null;

        /// <summary>
        /// Returns a strongly-typed API instance (convenience overload).
        /// </summary>
        public virtual T GetApi<T>() where T : class => GetApi() as T;
    }
}
