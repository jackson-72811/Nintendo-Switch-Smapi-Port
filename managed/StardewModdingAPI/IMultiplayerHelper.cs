using System.Collections.Generic;

namespace StardewModdingAPI
{
    /// <summary>Provides helpers for multiplayer functionality.</summary>
    public interface IMultiplayerHelper
    {
        /// <summary>The network ID of the current player in multiplayer.</summary>
        long GetNewID();

        /// <summary>Get connected peer information.</summary>
        IEnumerable<IMultiplayerPeer> GetConnectedPlayers();

        /// <summary>Get a connected peer by player ID.</summary>
        IMultiplayerPeer GetConnectedPlayer(long id);

        /// <summary>Send a message to connected players.</summary>
        void SendMessage<TMessage>(TMessage message, string messageType,
            string[] modIDs = null, long[] playerIDs = null);
    }

    /// <summary>Metadata about a connected multiplayer player.</summary>
    public interface IMultiplayerPeer
    {
        long         PlayerID   { get; }
        bool         IsHost     { get; }
        bool         IsSplitScreen { get; }
        ISemanticVersion GameVersion { get; }
        ISemanticVersion ApiVersion  { get; }
        IMultiplayerPeerMod GetMod(string id);
        IEnumerable<IMultiplayerPeerMod> Mods { get; }
    }

    /// <summary>A mod installed on a connected player's system.</summary>
    public interface IMultiplayerPeerMod
    {
        string           ID      { get; }
        ISemanticVersion Version { get; }
    }
}
