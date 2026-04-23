using System.Collections.Generic;
using Newtonsoft.Json;

namespace StardewModdingAPI
{
    /// <summary>Concrete implementation of <see cref="IManifest"/>.</summary>
    public class Manifest : IManifest
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("Author")]
        public string Author { get; set; }

        [JsonProperty("Version")]
        public ISemanticVersion Version { get; set; }

        [JsonProperty("MinimumApiVersion")]
        public ISemanticVersion MinimumApiVersion { get; set; }

        [JsonProperty("UniqueID")]
        public string UniqueID { get; set; }

        [JsonProperty("EntryDll")]
        public string EntryDll { get; set; }

        [JsonProperty("UpdateKeys")]
        public string UpdateKeys { get; set; }

        [JsonProperty("Dependencies")]
        public IManifestDependency[] Dependencies { get; set; } = System.Array.Empty<IManifestDependency>();

        [JsonProperty("ContentPackFor")]
        public string[] ContentPackFor { get; set; } = System.Array.Empty<string>();

        [JsonExtensionData]
        public Dictionary<string, object> ExtraFieldsDict { get; set; }

        public object ExtraFields => ExtraFieldsDict;
    }

    /// <summary>A manifest dependency.</summary>
    public class ManifestDependency : IManifestDependency
    {
        [JsonProperty("UniqueID")]
        public string UniqueID { get; set; }

        [JsonProperty("MinimumVersion")]
        public ISemanticVersion MinimumVersion { get; set; }

        [JsonProperty("IsRequired")]
        public bool IsRequired { get; set; } = true;
    }
}
