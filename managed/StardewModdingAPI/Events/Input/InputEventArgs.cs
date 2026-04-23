using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;

namespace StardewModdingAPI.Events.Input
{
    public class ButtonPressedEventArgs : EventArgs
    {
        public SButton          Button   { get; }
        public ICursorPosition  Cursor   { get; }
        internal ButtonPressedEventArgs(SButton btn, ICursorPosition cursor) {
            Button = btn; Cursor = cursor;
        }
    }

    public class ButtonReleasedEventArgs : EventArgs
    {
        public SButton          Button   { get; }
        public ICursorPosition  Cursor   { get; }
        internal ButtonReleasedEventArgs(SButton btn, ICursorPosition cursor) {
            Button = btn; Cursor = cursor;
        }
    }

    public class CursorMovedEventArgs : EventArgs
    {
        public ICursorPosition OldPosition { get; }
        public ICursorPosition NewPosition { get; }
        internal CursorMovedEventArgs(ICursorPosition oldPos, ICursorPosition newPos) {
            OldPosition = oldPos; NewPosition = newPos;
        }
    }

    public class MouseWheelScrolledEventArgs : EventArgs
    {
        public ICursorPosition Cursor    { get; }
        public int             OldValue  { get; }
        public int             NewValue  { get; }
        public int             Delta => NewValue - OldValue;
        internal MouseWheelScrolledEventArgs(ICursorPosition cursor, int oldVal, int newVal) {
            Cursor = cursor; OldValue = oldVal; NewValue = newVal;
        }
    }
}
