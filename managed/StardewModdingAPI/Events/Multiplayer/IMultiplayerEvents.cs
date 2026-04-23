using System;

namespace StardewModdingAPI.Events.Multiplayer
{
    public interface IMultiplayerEvents
    {
        event EventHandler<PeerContextReceivedEventArgs>  PeerContextReceived;
        event EventHandler<PeerConnectedEventArgs>        PeerConnected;
        event EventHandler<ModMessageReceivedEventArgs>   ModMessageReceived;
        event EventHandler<PeerDisconnectedEventArgs>     PeerDisconnected;
    }
}
