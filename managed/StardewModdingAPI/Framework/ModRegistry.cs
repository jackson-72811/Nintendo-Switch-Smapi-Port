using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewModdingAPI.Framework
{
    internal sealed class ModRegistry : IModRegistry
    {
        private readonly Dictionary<string, LoadedMod> _mods =
            new(StringComparer.OrdinalIgnoreCase);

        internal void RegisterMod(LoadedMod mod) {
            _mods[mod.Manifest.UniqueID] = mod;
        }

        public IModInfo Get(string uniqueID) {
            _mods.TryGetValue(uniqueID, out var mod);
            return mod != null ? new ModInfo(mod) : null;
        }

        public IEnumerable<IModInfo> GetAll() =>
            _mods.Values.Select(m => new ModInfo(m));

        public bool IsLoaded(string uniqueID) =>
            _mods.ContainsKey(uniqueID);

        public TApi GetApi<TApi>(string uniqueID) where TApi : class {
            if (!_mods.TryGetValue(uniqueID, out var mod)) return null;
            return mod.Instance.GetApi() as TApi;
        }
    }

    internal sealed class ModInfo : IModInfo
    {
        private readonly LoadedMod _mod;
        public IManifest Manifest      => _mod.Manifest;
        public bool      IsContentPack => false;

        public ModInfo(LoadedMod mod) { _mod = mod; }

        public IContentPack AsContentPack() =>
            throw new InvalidOperationException("This mod is not a content pack.");
    }
}
