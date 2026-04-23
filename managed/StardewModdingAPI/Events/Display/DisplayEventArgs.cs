using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewModdingAPI.Events.Display
{
    // Stub for game IClickableMenu
    public class IClickableMenu { }

    public class MenuChangedEventArgs : EventArgs
    {
        public IClickableMenu OldMenu { get; }
        public IClickableMenu NewMenu { get; }
        internal MenuChangedEventArgs(IClickableMenu oldMenu, IClickableMenu newMenu) {
            OldMenu = oldMenu; NewMenu = newMenu;
        }
    }

    public class RenderingEventArgs : EventArgs
    {
        public SpriteBatch SpriteBatch { get; }
        internal RenderingEventArgs(SpriteBatch sb) { SpriteBatch = sb; }
    }

    public class RenderedEventArgs : EventArgs
    {
        public SpriteBatch SpriteBatch { get; }
        internal RenderedEventArgs(SpriteBatch sb) { SpriteBatch = sb; }
    }

    public class RenderingWorldEventArgs : EventArgs
    {
        public SpriteBatch SpriteBatch { get; }
        internal RenderingWorldEventArgs(SpriteBatch sb) { SpriteBatch = sb; }
    }

    public class RenderedWorldEventArgs : EventArgs
    {
        public SpriteBatch SpriteBatch { get; }
        internal RenderedWorldEventArgs(SpriteBatch sb) { SpriteBatch = sb; }
    }

    public class RenderingActiveMenuEventArgs : EventArgs
    {
        public SpriteBatch SpriteBatch { get; }
        internal RenderingActiveMenuEventArgs(SpriteBatch sb) { SpriteBatch = sb; }
    }

    public class RenderedActiveMenuEventArgs : EventArgs
    {
        public SpriteBatch SpriteBatch { get; }
        internal RenderedActiveMenuEventArgs(SpriteBatch sb) { SpriteBatch = sb; }
    }

    public class RenderingHudEventArgs : EventArgs
    {
        public SpriteBatch SpriteBatch { get; }
        internal RenderingHudEventArgs(SpriteBatch sb) { SpriteBatch = sb; }
    }

    public class RenderedHudEventArgs : EventArgs
    {
        public SpriteBatch SpriteBatch { get; }
        internal RenderedHudEventArgs(SpriteBatch sb) { SpriteBatch = sb; }
    }

    public class WindowResizedEventArgs : EventArgs
    {
        public Point OldSize { get; }
        public Point NewSize { get; }
        internal WindowResizedEventArgs(Point oldSize, Point newSize) {
            OldSize = oldSize; NewSize = newSize;
        }
    }
}
