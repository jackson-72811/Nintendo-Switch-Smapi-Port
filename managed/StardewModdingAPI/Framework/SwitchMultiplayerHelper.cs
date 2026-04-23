using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StardewModdingAPI.Framework
{
    internal sealed class SwitchMultiplayerHelper : IMultiplayerHelper
    {
        private readonly IMonitor _monitor;
        private long _nextId = 1;

        public SwitchMultiplayerHelper(IMonitor monitor) { _monitor = monitor; }

        public long GetNewID() => _nextId++;

        public IEnumerable<IMultiplayerPeer> GetConnectedPlayers() =>
            Array.Empty<IMultiplayerPeer>();

        public IMultiplayerPeer GetConnectedPlayer(long id) => null;

        public void SendMessage<TMessage>(TMessage message, string messageType,
            string[] modIDs = null, long[] playerIDs = null) {
            // On Switch, multiplayer uses the game's existing net stack.
            // SMAPI messages are serialized and sent as in-game mod messages.
            _monitor.Log(
                $"[Multiplayer] SendMessage<{typeof(TMessage).Name}> type='{messageType}' "
                + "(Switch multiplayer dispatch not yet implemented)", LogLevel.Warn);
        }
    }
}
