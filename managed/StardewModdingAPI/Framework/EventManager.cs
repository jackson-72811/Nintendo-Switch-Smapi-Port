using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Events.GameLoop;
using StardewModdingAPI.Events.Input;
using StardewModdingAPI.Events.World;
using StardewModdingAPI.Events.Player;
using StardewModdingAPI.Events.Display;
using StardewModdingAPI.Events.Content;
using StardewModdingAPI.Events.Multiplayer;
using StardewModdingAPI.Events.Specialized;
using StardewModdingAPI.Utilities;

namespace StardewModdingAPI.Framework
{
    /// <summary>
    /// Central event manager.  Holds all event instances and provides the fire-*
    /// methods that SwitchGameHooks calls.
    /// </summary>
    internal sealed class EventManager :
        IModEvents,
        IGameLoopEvents, IInputEvents, IWorldEvents,
        IPlayerEvents, IDisplayEvents, IContentEvents,
        IMultiplayerEvents, ISpecializedEvents
    {
        private readonly IMonitor _monitor;

        public EventManager(IMonitor monitor) { _monitor = monitor; }

        // ── IModEvents ───────────────────────────────────────────────────────
        IGameLoopEvents    IModEvents.GameLoop    => this;
        IInputEvents       IModEvents.Input       => this;
        IWorldEvents       IModEvents.World       => this;
        IPlayerEvents      IModEvents.Player      => this;
        IDisplayEvents     IModEvents.Display     => this;
        IContentEvents     IModEvents.Content     => this;
        IMultiplayerEvents IModEvents.Multiplayer => this;
        ISpecializedEvents IModEvents.Specialized => this;

        // ═══════════════════════════════════════════════════════════════════════
        // GameLoop events
        // ═══════════════════════════════════════════════════════════════════════
        public event EventHandler<GameLaunchedEventArgs>         GameLaunched;
        public event EventHandler<UpdateTickingEventArgs>        UpdateTicking;
        public event EventHandler<UpdateTickedEventArgs>         UpdateTicked;
        public event EventHandler<OneSecondUpdateTickingEventArgs> OneSecondUpdateTicking;
        public event EventHandler<OneSecondUpdateTickedEventArgs>  OneSecondUpdateTicked;
        public event EventHandler<SaveCreatingEventArgs>         SaveCreating;
        public event EventHandler<SaveCreatedEventArgs>          SaveCreated;
        public event EventHandler<SavingEventArgs>               Saving;
        public event EventHandler<SavedEventArgs>                Saved;
        public event EventHandler<SaveLoadedEventArgs>           SaveLoaded;
        public event EventHandler<TimeChangedEventArgs>          TimeChanged;
        public event EventHandler<DayStartedEventArgs>           DayStarted;
        public event EventHandler<DayEndingEventArgs>            DayEnding;
        public event EventHandler<ReturnedToTitleEventArgs>      ReturnedToTitle;

        // ═══════════════════════════════════════════════════════════════════════
        // Input events
        // ═══════════════════════════════════════════════════════════════════════
        public event EventHandler<ButtonPressedEventArgs>        ButtonPressed;
        public event EventHandler<ButtonReleasedEventArgs>       ButtonReleased;
        public event EventHandler<CursorMovedEventArgs>          CursorMoved;
        public event EventHandler<MouseWheelScrolledEventArgs>   MouseWheelScrolled;

        // ═══════════════════════════════════════════════════════════════════════
        // World events
        // ═══════════════════════════════════════════════════════════════════════
        public event EventHandler<LocationListChangedEventArgs>       LocationListChanged;
        public event EventHandler<BuildingListChangedEventArgs>       BuildingListChanged;
        public event EventHandler<DebrisListChangedEventArgs>         DebrisListChanged;
        public event EventHandler<LargeTerrainFeatureListChangedEventArgs> LargeTerrainFeatureListChanged;
        public event EventHandler<NpcListChangedEventArgs>            NpcListChanged;
        public event EventHandler<ObjectListChangedEventArgs>         ObjectListChanged;
        public event EventHandler<ChestInventoryChangedEventArgs>     ChestInventoryChanged;
        public event EventHandler<TerrainFeatureListChangedEventArgs> TerrainFeatureListChanged;
        public event EventHandler<FurnitureListChangedEventArgs>      FurnitureListChanged;

        // ═══════════════════════════════════════════════════════════════════════
        // Player events
        // ═══════════════════════════════════════════════════════════════════════
        public event EventHandler<InventoryChangedEventArgs> InventoryChanged;
        public event EventHandler<LevelChangedEventArgs>     LevelChanged;
        public event EventHandler<WarpedEventArgs>           Warped;

        // ═══════════════════════════════════════════════════════════════════════
        // Display events
        // ═══════════════════════════════════════════════════════════════════════
        public event EventHandler<MenuChangedEventArgs>          MenuChanged;
        public event EventHandler<RenderingEventArgs>            Rendering;
        public event EventHandler<RenderedEventArgs>             Rendered;
        public event EventHandler<RenderingWorldEventArgs>       RenderingWorld;
        public event EventHandler<RenderedWorldEventArgs>        RenderedWorld;
        public event EventHandler<RenderingActiveMenuEventArgs>  RenderingActiveMenu;
        public event EventHandler<RenderedActiveMenuEventArgs>   RenderedActiveMenu;
        public event EventHandler<RenderingHudEventArgs>         RenderingHud;
        public event EventHandler<RenderedHudEventArgs>          RenderedHud;
        public event EventHandler<WindowResizedEventArgs>        WindowResized;

        // ═══════════════════════════════════════════════════════════════════════
        // Content events
        // ═══════════════════════════════════════════════════════════════════════
        public event EventHandler<AssetRequestedEventArgs>   AssetRequested;
        public event EventHandler<AssetReadyEventArgs>       AssetReady;
        public event EventHandler<AssetsInvalidatedEventArgs>AssetsInvalidated;
        public event EventHandler<LocaleChangedEventArgs>    LocaleChanged;

        // ═══════════════════════════════════════════════════════════════════════
        // Multiplayer events
        // ═══════════════════════════════════════════════════════════════════════
        public event EventHandler<PeerContextReceivedEventArgs> PeerContextReceived;
        public event EventHandler<PeerConnectedEventArgs>       PeerConnected;
        public event EventHandler<ModMessageReceivedEventArgs>  ModMessageReceived;
        public event EventHandler<PeerDisconnectedEventArgs>    PeerDisconnected;

        // ═══════════════════════════════════════════════════════════════════════
        // Specialized events
        // ═══════════════════════════════════════════════════════════════════════
        public event EventHandler<LoadStageChangedEventArgs>          LoadStageChanged;
        public event EventHandler<UnvalidatedUpdateTickingEventArgs>   UnvalidatedUpdateTicking;
        public event EventHandler<UnvalidatedUpdateTickedEventArgs>    UnvalidatedUpdateTicked;

        // ═══════════════════════════════════════════════════════════════════════
        // Fire helpers (called by SwitchGameHooks)
        // ═══════════════════════════════════════════════════════════════════════
        private void Fire<T>(EventHandler<T> handler, T args) where T : EventArgs {
            if (handler == null) return;
            foreach (var d in handler.GetInvocationList()) {
                try { ((EventHandler<T>)d)(this, args); }
                catch (Exception ex) {
                    _monitor.Log($"A mod event handler threw: {ex}", LogLevel.Error);
                }
            }
        }

        // GameLoop
        public void OnGameLaunched()    => Fire(GameLaunched, new GameLaunchedEventArgs());
        public void OnUpdateTicking(uint ticks) {
            Fire(UnvalidatedUpdateTicking, new UnvalidatedUpdateTickingEventArgs(ticks));
            Fire(UpdateTicking,            new UpdateTickingEventArgs(ticks));
            if (ticks % 60 == 0) Fire(OneSecondUpdateTicking, new OneSecondUpdateTickingEventArgs(ticks));
        }
        public void OnUpdateTicked(uint ticks) {
            Fire(UnvalidatedUpdateTicked, new UnvalidatedUpdateTickedEventArgs(ticks));
            Fire(UpdateTicked,            new UpdateTickedEventArgs(ticks));
            if (ticks % 60 == 0) Fire(OneSecondUpdateTicked, new OneSecondUpdateTickedEventArgs(ticks));
        }
        public void OnSaveCreating()    => Fire(SaveCreating, new SaveCreatingEventArgs());
        public void OnSaveCreated()     => Fire(SaveCreated,  new SaveCreatedEventArgs());
        public void OnSaving()          => Fire(Saving,       new SavingEventArgs());
        public void OnSaved()           => Fire(Saved,        new SavedEventArgs());
        public void OnSaveLoaded()      => Fire(SaveLoaded,   new SaveLoadedEventArgs());
        public void OnDayStarted()      => Fire(DayStarted,   new DayStartedEventArgs());
        public void OnDayEnding()       => Fire(DayEnding,    new DayEndingEventArgs());
        public void OnReturnedToTitle() => Fire(ReturnedToTitle, new ReturnedToTitleEventArgs());
        public void OnTimeChanged(int oldTime, int newTime)
            => Fire(TimeChanged, new TimeChangedEventArgs(oldTime, newTime));

        // Input
        public void OnButtonPressed(Utilities.SButton btn, ICursorPosition cursor)
            => Fire(ButtonPressed, new ButtonPressedEventArgs(btn, cursor));
        public void OnButtonReleased(Utilities.SButton btn, ICursorPosition cursor)
            => Fire(ButtonReleased, new ButtonReleasedEventArgs(btn, cursor));
        public void OnCursorMoved(ICursorPosition old, ICursorPosition @new)
            => Fire(CursorMoved, new CursorMovedEventArgs(old, @new));
        public void OnMouseWheelScrolled(ICursorPosition cursor, int oldVal, int newVal)
            => Fire(MouseWheelScrolled, new MouseWheelScrolledEventArgs(cursor, oldVal, newVal));

        // Display
        public void OnMenuChanged(IClickableMenu oldMenu, IClickableMenu newMenu)
            => Fire(MenuChanged, new MenuChangedEventArgs(oldMenu, newMenu));
        public void OnRendering(Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
            => Fire(Rendering, new RenderingEventArgs(sb));
        public void OnRendered(Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
            => Fire(Rendered, new RenderedEventArgs(sb));
        public void OnRenderingWorld(Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
            => Fire(RenderingWorld, new RenderingWorldEventArgs(sb));
        public void OnRenderedWorld(Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
            => Fire(RenderedWorld, new RenderedWorldEventArgs(sb));
        public void OnRenderingHud(Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
            => Fire(RenderingHud, new RenderingHudEventArgs(sb));
        public void OnRenderedHud(Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
            => Fire(RenderedHud, new RenderedHudEventArgs(sb));

        // Content
        public AssetRequestedEventArgs OnAssetRequested(IAssetName name, Func<object> defaultLoad) {
            var args = new AssetRequestedEventArgs(name, defaultLoad);
            Fire(AssetRequested, args);
            return args;
        }
        public void OnAssetReady(IAssetName name)
            => Fire(AssetReady, new AssetReadyEventArgs(name));
        public void OnAssetsInvalidated(IReadOnlySet<IAssetName> names)
            => Fire(AssetsInvalidated, new AssetsInvalidatedEventArgs(names));
        public void OnLocaleChanged(string oldLocale, string newLocale)
            => Fire(LocaleChanged, new LocaleChangedEventArgs(oldLocale, newLocale));

        // Load stage
        public void OnLoadStageChanged(LoadStage oldStage, LoadStage newStage)
            => Fire(LoadStageChanged, new LoadStageChangedEventArgs(oldStage, newStage));
    }
}
