using System;
using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events.Specialized;
using Events = StardewModdingAPI.Events;

namespace StardewModdingAPI.Framework
{
    /// <summary>
    /// Applies Harmony patches to the live Stardew Valley assembly to intercept
    /// game-loop callbacks and fire SMAPI events.
    ///
    /// All patch methods are static because Harmony requires static targets.
    /// They forward to the singleton <see cref="Instance"/> event manager.
    /// </summary>
    internal sealed class SwitchGameHooks
    {
        // ── Singleton state accessed by static patches ────────────────────────
        internal static EventManager Events   { get; private set; }
        internal static IMonitor     Monitor  { get; private set; }
        internal static uint         TickCount { get; private set; }
        private static int           _lastTime = -1;

        private Harmony _harmony;

        // ── Public API ────────────────────────────────────────────────────────

        public SwitchGameHooks(EventManager events, IMonitor monitor) {
            Events  = events;
            Monitor = monitor;
        }

        /// <summary>Apply all Harmony patches against the Stardew Valley assembly.</summary>
        public void ApplyPatches() {
            _harmony = new Harmony("smapi.switch.hooks");

            try {
                PatchGameLoop();
                PatchDraw();
                PatchSaveLoad();
                PatchTimeChange();
                Monitor.Log("Harmony patches applied successfully.", LogLevel.Info);
            } catch (Exception ex) {
                Monitor.Log($"Failed to apply Harmony patches: {ex}", LogLevel.Error);
                throw;
            }
        }

        public void RemovePatches() {
            _harmony?.UnpatchSelf();
        }

        // ── Patch: Game1.Update ───────────────────────────────────────────────

        private void PatchGameLoop() {
            var game1Type = FindGameType("StardewValley.Game1");
            if (game1Type == null) {
                Monitor.Log("Could not find StardewValley.Game1 — update loop patches skipped.", LogLevel.Warn);
                return;
            }

            var updateMethod = game1Type.GetMethod("Update",
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic,
                null, new[] { typeof(GameTime) }, null);

            if (updateMethod != null) {
                _harmony.Patch(
                    updateMethod,
                    prefix:  new HarmonyMethod(typeof(SwitchGameHooks), nameof(Game1_Update_Prefix)),
                    postfix: new HarmonyMethod(typeof(SwitchGameHooks), nameof(Game1_Update_Postfix)));
            }
        }

        // ── Patch: Game1.Draw ─────────────────────────────────────────────────

        private void PatchDraw() {
            var game1Type = FindGameType("StardewValley.Game1");
            if (game1Type == null) return;

            var drawMethod = game1Type.GetMethod("Draw",
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic,
                null, new[] { typeof(GameTime) }, null);

            if (drawMethod != null) {
                _harmony.Patch(
                    drawMethod,
                    prefix:  new HarmonyMethod(typeof(SwitchGameHooks), nameof(Game1_Draw_Prefix)),
                    postfix: new HarmonyMethod(typeof(SwitchGameHooks), nameof(Game1_Draw_Postfix)));
            }
        }

        // ── Patch: SaveGame methods ───────────────────────────────────────────

        private void PatchSaveLoad() {
            var saveGameType = FindGameType("StardewValley.SaveGame");
            if (saveGameType == null) return;

            TryPatch(saveGameType, "Load", null,
                new HarmonyMethod(typeof(SwitchGameHooks), nameof(SaveGame_Load_Prefix)),
                new HarmonyMethod(typeof(SwitchGameHooks), nameof(SaveGame_Load_Postfix)));

            TryPatch(saveGameType, "Save", null,
                new HarmonyMethod(typeof(SwitchGameHooks), nameof(SaveGame_Save_Prefix)),
                new HarmonyMethod(typeof(SwitchGameHooks), nameof(SaveGame_Save_Postfix)));
        }

        // ── Patch: in-game time ───────────────────────────────────────────────

        private void PatchTimeChange() {
            var game1Type = FindGameType("StardewValley.Game1");
            if (game1Type == null) return;

            // SDV uses performTenMinuteClockUpdate for time advancement
            TryPatch(game1Type, "performTenMinuteClockUpdate", null, null,
                new HarmonyMethod(typeof(SwitchGameHooks), nameof(Game1_TimeUpdate_Postfix)));
        }

        // ── Harmony patch methods (must be static) ────────────────────────────

        [HarmonyPrefix]
        private static void Game1_Update_Prefix() {
            Events?.OnUpdateTicking(TickCount);
        }

        [HarmonyPostfix]
        private static void Game1_Update_Postfix() {
            Events?.OnUpdateTicked(TickCount);
            TickCount++;
        }

        [HarmonyPrefix]
        private static void Game1_Draw_Prefix(SpriteBatch ____spriteBatch) {
            Events?.OnRendering(____spriteBatch);
        }

        [HarmonyPostfix]
        private static void Game1_Draw_Postfix(SpriteBatch ____spriteBatch) {
            Events?.OnRendered(____spriteBatch);
        }

        [HarmonyPrefix]
        private static void SaveGame_Load_Prefix() {
            Events?.OnLoadStageChanged(LoadStage.None, LoadStage.SaveParsed);
        }

        [HarmonyPostfix]
        private static void SaveGame_Load_Postfix() {
            Events?.OnSaveLoaded();
            Events?.OnDayStarted();
            Events?.OnLoadStageChanged(LoadStage.SaveLoadedBasicInfo, LoadStage.Loaded);
        }

        [HarmonyPrefix]
        private static void SaveGame_Save_Prefix() {
            Events?.OnSaving();
        }

        [HarmonyPostfix]
        private static void SaveGame_Save_Postfix() {
            Events?.OnSaved();
        }

        [HarmonyPostfix]
        private static void Game1_TimeUpdate_Postfix() {
            // Read current in-game time via reflection
            try {
                var game1Type = FindGameType("StardewValley.Game1");
                if (game1Type == null) return;
                var timeProp = game1Type.GetField("timeOfDay",
                    BindingFlags.Public | BindingFlags.Static);
                if (timeProp == null) return;

                int newTime = (int)timeProp.GetValue(null);
                if (newTime != _lastTime) {
                    Events?.OnTimeChanged(_lastTime, newTime);
                    _lastTime = newTime;
                }
            } catch { /* time reflection is best-effort */ }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static Type FindGameType(string fullName) {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
                var t = asm.GetType(fullName);
                if (t != null) return t;
            }
            return null;
        }

        private void TryPatch(Type type, string methodName, Type[] paramTypes,
                               HarmonyMethod prefix, HarmonyMethod postfix) {
            try {
                MethodInfo m = paramTypes != null
                    ? type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance
                                               | BindingFlags.Static | BindingFlags.NonPublic,
                                     null, paramTypes, null)
                    : type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance
                                               | BindingFlags.Static | BindingFlags.NonPublic);
                if (m != null)
                    _harmony.Patch(m, prefix, postfix);
            } catch (Exception ex) {
                Monitor.Log($"Could not patch {type.Name}.{methodName}: {ex.Message}", LogLevel.Warn);
            }
        }
    }
}
