using Microsoft.Xna.Framework.Input;

namespace StardewModdingAPI.Utilities
{
    /// <summary>A unified button identifier that covers keyboard, mouse, and controller inputs.</summary>
    public enum SButton
    {
        None = 0,

        // ── Keyboard ──────────────────────────────────────────────────────
        Escape = Keys.Escape,
        F1 = Keys.F1, F2 = Keys.F2, F3 = Keys.F3, F4 = Keys.F4,
        F5 = Keys.F5, F6 = Keys.F6, F7 = Keys.F7, F8 = Keys.F8,
        F9 = Keys.F9, F10 = Keys.F10, F11 = Keys.F11, F12 = Keys.F12,
        Tab = Keys.Tab,
        CapsLock = Keys.CapsLock,
        LeftShift = Keys.LeftShift, RightShift = Keys.RightShift,
        LeftControl = Keys.LeftControl, RightControl = Keys.RightControl,
        LeftAlt = Keys.LeftAlt, RightAlt = Keys.RightAlt,
        Space = Keys.Space, Enter = Keys.Enter, Back = Keys.Back,
        Delete = Keys.Delete, Insert = Keys.Insert,
        Home = Keys.Home, End = Keys.End,
        PageUp = Keys.PageUp, PageDown = Keys.PageDown,
        Up = Keys.Up, Down = Keys.Down, Left = Keys.Left, Right = Keys.Right,
        A = Keys.A, B = Keys.B, C = Keys.C, D = Keys.D, E = Keys.E,
        F = Keys.F, G = Keys.G, H = Keys.H, I = Keys.I, J = Keys.J,
        K = Keys.K, L = Keys.L, M = Keys.M, N = Keys.N, O = Keys.O,
        P = Keys.P, Q = Keys.Q, R = Keys.R, S = Keys.S, T = Keys.T,
        U = Keys.U, V = Keys.V, W = Keys.W, X = Keys.X, Y = Keys.Y, Z = Keys.Z,
        D0 = Keys.D0, D1 = Keys.D1, D2 = Keys.D2, D3 = Keys.D3, D4 = Keys.D4,
        D5 = Keys.D5, D6 = Keys.D6, D7 = Keys.D7, D8 = Keys.D8, D9 = Keys.D9,

        // ── Mouse ─────────────────────────────────────────────────────────
        MouseLeft    = 1000,
        MouseRight   = 1001,
        MouseMiddle  = 1002,
        MouseX1      = 1003,
        MouseX2      = 1004,

        // ── Controller (Switch buttons) ───────────────────────────────────
        ControllerA            = 2000,
        ControllerB            = 2001,
        ControllerX            = 2002,
        ControllerY            = 2003,
        ControllerBack         = 2004,  // Minus on Switch
        ControllerBigButton    = 2005,  // Home
        ControllerStart        = 2006,  // Plus on Switch
        ControllerStickLeft    = 2007,
        ControllerStickRight   = 2008,
        ControllerShoulderLeft = 2009,
        ControllerShoulderRight= 2010,
        ControllerTriggerLeft  = 2011,
        ControllerTriggerRight = 2012,
        ControllerDPadUp       = 2013,
        ControllerDPadDown     = 2014,
        ControllerDPadLeft     = 2015,
        ControllerDPadRight    = 2016,
    }

    public static class SButtonExtensions
    {
        public static bool IsUseToolButton(this SButton button)
            => button is SButton.MouseLeft or SButton.ControllerX;

        public static bool IsActionButton(this SButton button)
            => button is SButton.MouseRight or SButton.ControllerA;

        public static bool TryGetKeyboard(this SButton button, out Keys key) {
            if ((int)button is >= (int)Keys.Back and <= (int)Keys.OemClear) {
                key = (Keys)(int)button; return true;
            }
            key = Keys.None; return false;
        }

        public static bool TryGetController(this SButton button, out Buttons controllerButton) {
            controllerButton = default;
            if (button < SButton.ControllerA) return false;
            switch (button) {
                case SButton.ControllerA:             controllerButton = Buttons.A;            return true;
                case SButton.ControllerB:             controllerButton = Buttons.B;            return true;
                case SButton.ControllerX:             controllerButton = Buttons.X;            return true;
                case SButton.ControllerY:             controllerButton = Buttons.Y;            return true;
                case SButton.ControllerBack:          controllerButton = Buttons.Back;         return true;
                case SButton.ControllerStart:         controllerButton = Buttons.Start;        return true;
                case SButton.ControllerStickLeft:     controllerButton = Buttons.LeftStick;    return true;
                case SButton.ControllerStickRight:    controllerButton = Buttons.RightStick;   return true;
                case SButton.ControllerShoulderLeft:  controllerButton = Buttons.LeftShoulder; return true;
                case SButton.ControllerShoulderRight: controllerButton = Buttons.RightShoulder;return true;
                case SButton.ControllerDPadUp:        controllerButton = Buttons.DPadUp;       return true;
                case SButton.ControllerDPadDown:      controllerButton = Buttons.DPadDown;     return true;
                case SButton.ControllerDPadLeft:      controllerButton = Buttons.DPadLeft;     return true;
                case SButton.ControllerDPadRight:     controllerButton = Buttons.DPadRight;    return true;
                default: return false;
            }
        }
    }
}
