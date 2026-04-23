using System.Collections.Generic;

namespace StardewModdingAPI
{
    /// <summary>A manifest which describes a mod for SMAPI.</summary>
    public interface IManifest
    {
        string Name                { get; }
        string Description         { get; }
        string Author              { get; }
        ISemanticVersion Version   { get; }
        ISemanticVersion MinimumApiVersion { get; }
        string UniqueID            { get; }
        string EntryDll            { get; }
        object ExtraFields         { get; }
        IManifestDependency[] Dependencies { get; }
        string[] ContentPackFor    { get; }
        string UpdateKeys          { get; }
    }

    /// <summary>A dependency listed in a mod manifest.</summary>
    public interface IManifestDependency
    {
        string UniqueID            { get; }
        ISemanticVersion MinimumVersion { get; }
        bool IsRequired            { get; }
    }
}
