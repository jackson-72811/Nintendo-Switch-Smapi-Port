using StardewModdingAPI.Events.GameLoop;
using StardewModdingAPI.Events.Input;
using StardewModdingAPI.Events.World;
using StardewModdingAPI.Events.Player;
using StardewModdingAPI.Events.Display;
using StardewModdingAPI.Events.Content;
using StardewModdingAPI.Events.Multiplayer;
using StardewModdingAPI.Events.Specialized;

namespace StardewModdingAPI.Events
{
    /// <summary>Manages access to events raised by SMAPI and the game.</summary>
    public interface IModEvents
    {
        IGameLoopEvents    GameLoop    { get; }
        IInputEvents       Input       { get; }
        IWorldEvents       World       { get; }
        IPlayerEvents      Player      { get; }
        IDisplayEvents     Display     { get; }
        IContentEvents     Content     { get; }
        IMultiplayerEvents Multiplayer { get; }
        ISpecializedEvents Specialized { get; }
    }
}
