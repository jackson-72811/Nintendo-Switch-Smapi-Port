using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI.Utilities;

namespace StardewModdingAPI
{
    /// <summary>Provides helpers to read the current input state.</summary>
    public interface IInputHelper
    {
        /// <summary>Get the current mouse position on screen.</summary>
        ICursorPosition GetCursorPosition();

        /// <summary>Get the current state of a button.</summary>
        SButtonState GetState(SButton button);

        /// <summary>Returns true if the button was just pressed this tick.</summary>
        bool IsDown(SButton button);

        /// <summary>Suppress the given button's press event for the current tick.</summary>
        void Suppress(SButton button);

        /// <summary>Returns true if the button was suppressed this tick.</summary>
        bool IsSuppressed(SButton button);
    }

    /// <summary>The state of a button.</summary>
    public enum SButtonState
    {
        None,
        Pressed,
        Held,
        Released
    }

    /// <summary>The current cursor position.</summary>
    public interface ICursorPosition
    {
        Vector2 AbsolutePixels  { get; }
        Vector2 ScreenPixels    { get; }
        Vector2 Tile            { get; }
        Vector2 GrabTile        { get; }
    }
}
