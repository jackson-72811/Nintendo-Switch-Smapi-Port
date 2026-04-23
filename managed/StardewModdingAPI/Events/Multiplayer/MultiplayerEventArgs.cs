using System;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;

namespace StardewModdingAPI.Events.Multiplayer
{
    public class PeerContextReceivedEventArgs : EventArgs
    {
        public IMultiplayerPeer Peer { get; }
        internal PeerContextReceivedEventArgs(IMultiplayerPeer peer) { Peer = peer; }
    }

    public class PeerConnectedEventArgs : EventArgs
    {
        public IMultiplayerPeer Peer { get; }
        internal PeerConnectedEventArgs(IMultiplayerPeer peer) { Peer = peer; }
    }

    public class PeerDisconnectedEventArgs : EventArgs
    {
        public IMultiplayerPeer Peer { get; }
        internal PeerDisconnectedEventArgs(IMultiplayerPeer peer) { Peer = peer; }
    }

    public class ModMessageReceivedEventArgs : EventArgs
    {
        private readonly JToken _data;

        public long   FromPlayerID { get; }
        public string FromModID    { get; }
        public string Type         { get; }

        internal ModMessageReceivedEventArgs(long fromPlayer, string fromMod,
                                              string type, JToken data) {
            FromPlayerID = fromPlayer;
            FromModID    = fromMod;
            Type         = type;
            _data        = data;
        }

        public TMessage ReadAs<TMessage>() => _data.ToObject<TMessage>();
    }
}
