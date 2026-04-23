using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events.Content;
using StardewModdingAPI.Utilities;

namespace StardewModdingAPI.Framework
{
    /// <summary>
    /// Game content helper for the Switch.  Routes asset loads through the
    /// AssetRequested event so mods can intercept/replace assets.
    /// </summary>
    internal sealed class SwitchContentHelper : IGameContentHelper
    {
        private readonly string       _gamePath;
        private readonly EventManager _events;
        private readonly IMonitor     _monitor;
        private readonly Dictionary<string, object> _cache = new(StringComparer.OrdinalIgnoreCase);
        private string _locale = "en";

        public string CurrentLocale => _locale;

        public SwitchContentHelper(string gamePath, EventManager events, IMonitor monitor) {
            _gamePath = gamePath;
            _events   = events;
            _monitor  = monitor;
        }

        public IAssetName ParseAssetName(string rawName) => AssetName.Parse(rawName);

        public T Load<T>(string assetName) where T : class
            => Load<T>(AssetName.Parse(assetName));

        public T Load<T>(IAssetName assetName) where T : class {
            // Check mod-supplied loads via AssetRequested event
            var args = _events.OnAssetRequested(assetName, () => LoadFromDisk<T>(assetName));

            object data;
            if (args.HasLoad) {
                data = args.InvokeLoad();
            } else {
                // Load from game content or disk
                data = LoadFromDisk<T>(assetName);
                if (data == null) return null;
            }

            if (args.HasEdit)
                args.InvokeEdit(data);

            _events.OnAssetReady(assetName);
            return (T)data;
        }

        public void InvalidateCache(string assetName)
            => InvalidateCache(AssetName.Parse(assetName));

        public void InvalidateCache(IAssetName assetName) {
            string key = assetName.Name.ToLowerInvariant();
            _cache.Remove(key);
            _events.OnAssetsInvalidated(
                new HashSet<IAssetName> { assetName });
        }

        public bool InvalidateCache(Func<IAssetName, bool> predicate) {
            var invalidated = new HashSet<IAssetName>();
            foreach (string key in new List<string>(_cache.Keys)) {
                var name = AssetName.Parse(key);
                if (predicate(name)) {
                    _cache.Remove(key);
                    invalidated.Add(name);
                }
            }
            if (invalidated.Count > 0)
                _events.OnAssetsInvalidated(invalidated);
            return invalidated.Count > 0;
        }

        public bool DoesAssetExist<T>(IAssetName assetName) where T : class {
            string path = ResolveAssetPath(assetName.BaseName);
            return path != null && File.Exists(path);
        }

        public IAssetData GetPatchHelper<T>(T data, string assetName = null) where T : class {
            var name = assetName != null ? AssetName.Parse(assetName) : new AssetName("_temp");
            return new AssetDataWrapper<T>(data, name);
        }

        // ── Internal ─────────────────────────────────────────────────────────

        private T LoadFromDisk<T>(IAssetName assetName) where T : class {
            // Check mod overlay (LayeredFS) first, then game content
            string path = ResolveAssetPath(assetName.BaseName);
            if (path == null) return null;

            try {
                if (typeof(T) == typeof(Microsoft.Xna.Framework.Graphics.Texture2D) ||
                    typeof(T).Name.Contains("Texture")) {
                    return LoadTexture<T>(path);
                }

                // Default: try JSON, then XNB-style string
                string ext = Path.GetExtension(path).ToLower();
                if (ext == ".json") {
                    return JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
                }

                // Return raw bytes / stream for unknown types
                return null;
            } catch (Exception ex) {
                _monitor.Log($"Failed to load asset '{assetName}': {ex.Message}", LogLevel.Error);
                return null;
            }
        }

        private T LoadTexture<T>(string path) where T : class {
            // On Switch, textures are loaded via MonoGame's content system
            // which is already available in the game's assembly
            try {
                using var stream = File.OpenRead(path);
                var tex = Microsoft.Xna.Framework.Graphics.Texture2D.FromStream(
                    GetGraphicsDevice(), stream);
                return (T)(object)tex;
            } catch {
                return null;
            }
        }

        private string ResolveAssetPath(string baseName) {
            // Normalize: Maps\Town -> Maps/Town
            baseName = baseName.Replace('\\', '/');

            // Mod overlay path (Atmosphere LayeredFS)
            string[] searchPaths = {
                Path.Combine(Constants.InternalFilesPath, "ModContent", baseName + ".png"),
                Path.Combine(Constants.InternalFilesPath, "ModContent", baseName + ".json"),
                Path.Combine(Constants.InternalFilesPath, "ModContent", baseName + ".xnb"),
                Path.Combine(_gamePath, "Content", baseName + ".xnb"),
                Path.Combine(_gamePath, "Content", baseName + ".json"),
            };

            foreach (string p in searchPaths) {
                if (File.Exists(p)) return p;
            }
            return null;
        }

        internal static Microsoft.Xna.Framework.Graphics.GraphicsDevice GetGraphicsDeviceStatic()
            => GetGraphicsDevice();

        private static Microsoft.Xna.Framework.Graphics.GraphicsDevice GetGraphicsDevice() {
            // Retrieve from the running game instance via reflection
            try {
                var game1Type = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectType("StardewValley.Game1");
                if (game1Type == null) return null;
                var prop = game1Type.GetProperty("graphics",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                dynamic graphicsMgr = prop?.GetValue(null);
                return graphicsMgr?.GraphicsDevice;
            } catch {
                return null;
            }
        }
    }

    internal static class AssemblyExtensions
    {
        public static Type SelectType(this System.Reflection.Assembly[] asms, string fullName) {
            foreach (var asm in asms) {
                var t = asm.GetType(fullName);
                if (t != null) return t;
            }
            return null;
        }
    }

    internal sealed class AssetDataWrapper<T> : IAssetData<T> where T : class
    {
        public IAssetName Name     { get; }
        public Type       DataType => typeof(T);
        public T          Data     { get; set; }
        object IAssetData.Data     => Data;

        public AssetDataWrapper(T data, IAssetName name) { Data = data; Name = name; }

        public IAssetData<TData> GetData<TData>() where TData : class
            => new AssetDataWrapper<TData>((TData)(object)Data, Name);

        public void ReplaceWith(T newData) => Data = newData;
        public void Edit(Action<T> editor) => editor(Data);
    }
}
