using System;

namespace StardewModdingAPI.Events.Input
{
    public interface IInputEvents
    {
        event EventHandler<ButtonPressedEventArgs>  ButtonPressed;
        event EventHandler<ButtonReleasedEventArgs> ButtonReleased;
        event EventHandler<CursorMovedEventArgs>    CursorMoved;
        event EventHandler<MouseWheelScrolledEventArgs> MouseWheelScrolled;
    }
}
