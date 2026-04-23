using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace StardewModdingAPI.Framework
{
    internal sealed class SwitchInputHelper : IInputHelper
    {
        private readonly HashSet<SButton> _suppressed = new();
        private KeyboardState    _prevKb;
        private GamePadState     _prevGp;
        private Vector2          _prevCursorPos;

        public ICursorPosition GetCursorPosition() {
            var mouse = Mouse.GetState();
            return new CursorPosition(new Vector2(mouse.X, mouse.Y));
        }

        public SButtonState GetState(SButton button) {
            if (button.TryGetKeyboard(out var key)) {
                var curKb  = Keyboard.GetState();
                bool down  = curKb.IsKeyDown(key);
                bool wasDown = _prevKb.IsKeyDown(key);
                return (down, wasDown) switch {
                    (true,  false) => SButtonState.Pressed,
                    (true,  true)  => SButtonState.Held,
                    (false, true)  => SButtonState.Released,
                    _              => SButtonState.None
                };
            }

            if (button.TryGetController(out var btn)) {
                var curGp  = GamePad.GetState(0);
                bool down  = curGp.IsButtonDown(btn);
                bool wasDown = _prevGp.IsButtonDown(btn);
                return (down, wasDown) switch {
                    (true,  false) => SButtonState.Pressed,
                    (true,  true)  => SButtonState.Held,
                    (false, true)  => SButtonState.Released,
                    _              => SButtonState.None
                };
            }

            return SButtonState.None;
        }

        public bool IsDown(SButton button) {
            var s = GetState(button);
            return s is SButtonState.Pressed or SButtonState.Held;
        }

        public void Suppress(SButton button) => _suppressed.Add(button);
        public bool IsSuppressed(SButton button) => _suppressed.Contains(button);

        // Called by the game-hook Update postfix to advance the state snapshot
        internal void Update() {
            _prevKb = Keyboard.GetState();
            _prevGp = GamePad.GetState(0);
            _suppressed.Clear();
        }
    }

    internal sealed class CursorPosition : ICursorPosition
    {
        public Vector2 AbsolutePixels { get; }
        public Vector2 ScreenPixels   => AbsolutePixels;
        public Vector2 Tile           => new(
            (float)Math.Floor(AbsolutePixels.X / 64),
            (float)Math.Floor(AbsolutePixels.Y / 64));
        public Vector2 GrabTile       => Tile;

        public CursorPosition(Vector2 pixels) { AbsolutePixels = pixels; }
    }
}
