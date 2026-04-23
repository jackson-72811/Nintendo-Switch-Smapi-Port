using System;

namespace StardewModdingAPI.Events.Display
{
    public interface IDisplayEvents
    {
        event EventHandler<MenuChangedEventArgs>          MenuChanged;
        event EventHandler<RenderingEventArgs>            Rendering;
        event EventHandler<RenderedEventArgs>             Rendered;
        event EventHandler<RenderingWorldEventArgs>       RenderingWorld;
        event EventHandler<RenderedWorldEventArgs>        RenderedWorld;
        event EventHandler<RenderingActiveMenuEventArgs>  RenderingActiveMenu;
        event EventHandler<RenderedActiveMenuEventArgs>   RenderedActiveMenu;
        event EventHandler<RenderingHudEventArgs>         RenderingHud;
        event EventHandler<RenderedHudEventArgs>          RenderedHud;
        event EventHandler<WindowResizedEventArgs>        WindowResized;
    }
}
