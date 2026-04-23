namespace StardewModdingAPI
{
    /// <summary>Encapsulates monitoring and logging for a mod.</summary>
    public interface IMonitor
    {
        /// <summary>Whether verbose logging is enabled for this mod.</summary>
        bool IsVerbose { get; }

        /// <summary>Log a message for the player or developer.</summary>
        void Log(string message, LogLevel level = LogLevel.Trace);

        /// <summary>Log a message that will only appear when verbose logging is enabled.</summary>
        void VerboseLog(string message);

        /// <summary>Log a message once, even if the same message is logged multiple times.</summary>
        void LogOnce(string message, LogLevel level = LogLevel.Trace);
    }
}
