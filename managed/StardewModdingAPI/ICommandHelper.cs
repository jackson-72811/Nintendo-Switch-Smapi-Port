using System;

namespace StardewModdingAPI
{
    /// <summary>Provides helpers to register SMAPI console commands.</summary>
    public interface ICommandHelper
    {
        /// <summary>Register a console command.</summary>
        void Add(string name, string documentation, Action<string, string[]> callback);

        /// <summary>Trigger a console command programmatically.</summary>
        bool Trigger(string name, string[] args);
    }
}
