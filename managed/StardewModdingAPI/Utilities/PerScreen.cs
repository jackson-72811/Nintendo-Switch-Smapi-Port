using System;
using System.Collections.Generic;

namespace StardewModdingAPI.Utilities
{
    /// <summary>
    /// A value wrapper that maintains separate instances per screen in split-screen
    /// multiplayer.  On Switch (single-screen / single-player) this always returns
    /// the same instance.
    /// </summary>
    public class PerScreen<T>
    {
        private readonly Func<T>    _createNew;
        private readonly Dictionary<int, T> _values = new();

        public PerScreen() : this(() => default) { }
        public PerScreen(Func<T> createNew) {
            _createNew = createNew ?? throw new ArgumentNullException(nameof(createNew));
        }

        /// <summary>The value for the current screen.</summary>
        public T Value {
            get {
                int screenId = GetCurrentScreenId();
                if (!_values.TryGetValue(screenId, out T val)) {
                    val = _createNew();
                    _values[screenId] = val;
                }
                return val;
            }
            set => _values[GetCurrentScreenId()] = value;
        }

        /// <summary>Get all active values across all screens.</summary>
        public IEnumerable<T> GetActiveValues() => _values.Values;

        private static int GetCurrentScreenId() => 0; // Single screen on Switch
    }
}
