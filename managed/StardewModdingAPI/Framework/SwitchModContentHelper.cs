using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI.Utilities;

namespace StardewModdingAPI.Framework
{
    internal sealed class SwitchModContentHelper : IModContentHelper
    {
        private readonly string   _modDir;
        private readonly IManifest _manifest;

        public SwitchModContentHelper(string modDir, IManifest manifest) {
            _modDir   = modDir;
            _manifest = manifest;
        }

        public T Load<T>(string relativePath) where T : class {
            string fullPath = Path.Combine(_modDir, relativePath);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Mod content file not found: '{relativePath}'", fullPath);

            string ext = Path.GetExtension(fullPath).ToLowerInvariant();
            object result = ext switch {
                ".png" or ".jpg"
                    => LoadTexture(fullPath),
                ".json"
                    => JsonConvert.DeserializeObject<T>(File.ReadAllText(fullPath)),
                ".csv"
                    => File.ReadAllText(fullPath),
                _   => File.ReadAllBytes(fullPath)
            };

            return result as T;
        }

        public IAssetName GetInternalAssetName(string relativePath)
            => new AssetName($"Mods/{_manifest.UniqueID}/{relativePath.Replace('\\', '/')}");

        public IAssetName GetManifestAssetName(string relativePath)
            => GetInternalAssetName(relativePath);

        private static object LoadTexture(string path) {
            // MonoGame texture loading
            try {
                using var stream = File.OpenRead(path);
                // Texture2D.FromStream requires a GraphicsDevice — retrieve it
                var gd = SwitchContentHelper.GetGraphicsDeviceStatic();
                if (gd == null) throw new InvalidOperationException("No GraphicsDevice available");
                return Texture2D.FromStream(gd, stream);
            } catch (Exception ex) {
                throw new InvalidOperationException($"Failed to load texture '{path}': {ex.Message}", ex);
            }
        }
    }
}
